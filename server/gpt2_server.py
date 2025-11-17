from flask import Flask, request, jsonify
from flask_cors import CORS
import sys
import json
import time
import logging
from datetime import datetime
from queue import Queue, Empty
import threading
import uuid

# Configuration

app = Flask(__name__)
CORS(app)

model_loaded = False
model = None
tokenizer = None
request_queue = Queue(maxsize=20)
processing_active = False

SERVER_PORT = int(sys.argv[1]) if len(sys.argv) > 1 else 5000

# Logging setup

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s [%(levelname)s] %(message)s',
    handlers=[
        logging.FileHandler(f'server_{datetime.now():%Y%m%d}.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

# Model loading

def load_model():
    global model_loaded, model, tokenizer
    
    try:
        logger.info("="*60)
        logger.info("LOADING GPT-2 MODEL")
        logger.info("="*60)
        
        from transformers import GPT2LMHeadModel, GPT2Tokenizer
        import torch
        
        model_path = "../gpt2"
        
        logger.info(f"Loading tokenizer from {model_path}...")
        tokenizer = GPT2Tokenizer.from_pretrained(model_path)
        tokenizer.pad_token = tokenizer.eos_token
        
        logger.info(f"Loading model from {model_path}...")
        model = GPT2LMHeadModel.from_pretrained(model_path)
        model.eval()
        
        if torch.cuda.is_available():
            logger.info("CUDA available - using GPU with FP16")
            model = model.half().to('cuda')
        else:
            logger.info("CUDA not available - using CPU")
        
        model_loaded = True
        logger.info("Model loaded successfully")
        logger.info("="*60)
        
    except Exception as e:
        logger.error(f"Model loading failed: {e}", exc_info=True)
        model_loaded = False

# Request processing

def generate_response_impl(request_data):
    import torch
    
    personality = request_data.get('personality', 'aya')
    player_input = request_data.get('player_input', '')
    memory_context = request_data.get('memory_context', '')
    max_tokens = request_data.get('max_tokens', 30)
    temperature = request_data.get('temperature', 0.85)
    
    personality_prompts = {
        'aya': "Aya is cheerful and sweet.\n",
        'mika': "Mika is quiet and shy.\n",
        'sora': "Sora is playful and energetic.\n"
    }
    
    system_prompt = personality_prompts.get(personality.lower(), personality_prompts['aya'])
    
    prompt = system_prompt
    if memory_context:
        lines = memory_context.strip().split('\n')[-2:]
        prompt += '\n'.join(lines) + '\n'
    prompt += f"Player: {player_input}\n{personality.capitalize()}:"
    
    inputs = tokenizer.encode(
        prompt,
        return_tensors='pt',
        max_length=512,
        truncation=True
    )
    
    if torch.cuda.is_available():
        inputs = inputs.to('cuda')
    
    with torch.no_grad():
        outputs = model.generate(
            inputs,
            max_new_tokens=max_tokens,
            temperature=temperature,
            do_sample=True,
            top_k=50,
            top_p=0.9,
            repetition_penalty=1.15,
            num_return_sequences=1,
            pad_token_id=tokenizer.eos_token_id,
            eos_token_id=tokenizer.eos_token_id,
            no_repeat_ngram_size=3,
            use_cache=True,
            early_stopping=True
        )
    
    generated_text = tokenizer.decode(outputs[0], skip_special_tokens=True)
    
    response_start = generated_text.rfind(f"{personality.capitalize()}:")
    if response_start != -1:
        response = generated_text[response_start + len(f"{personality.capitalize()}:"):].strip()
    else:
        response = generated_text.strip()
    
    response = response.split("Player:")[0].strip()
    response = response.split("\n")[0].strip()
    
    import re
    response = re.sub(r'\b(\w+)( \1){3,}', r'\1', response)
    
    if len(response) > 100:
        for i in range(100, 50, -1):
            if response[i] in '.!?':
                response = response[:i+1]
                break
        else:
            response = response[:100].rsplit(' ', 1)[0]
    
    response = response.strip()
    if not response:
        fallbacks = {
            'aya': "Hmm? ♡",
            'mika': "...",
            'sora': "Huh?"
        }
        response = fallbacks.get(personality.lower(), "...")
    
    import random
    positive_words = ['love', 'happy', 'yes', 'great', 'thanks', 'nice']
    negative_words = ['no', 'bad', 'hate', 'leave', 'bye']
    
    input_lower = player_input.lower()
    intimacy_delta = 0
    
    if any(word in input_lower for word in positive_words):
        intimacy_delta = random.randint(2, 5)
    elif any(word in input_lower for word in negative_words):
        intimacy_delta = random.randint(-3, -1)
    else:
        intimacy_delta = random.randint(-1, 2)
    
    intimacy_score = random.randint(40, 100)
    
    if intimacy_score >= 80:
        intimacy_level = "Lover"
    elif intimacy_score >= 50:
        intimacy_level = "Friend"
    else:
        intimacy_level = "Acquaintance"
    
    return {
        "response": response,
        "intimacy_delta": intimacy_delta,
        "intimacy_score": intimacy_score,
        "intimacy_level": intimacy_level,
        "fallback": False
    }

def process_request_queue():
    global processing_active
    processing_active = True
    
    logger.info("Request processing thread started")
    
    while processing_active:
        try:
            request_id, request_data, result_container = request_queue.get(timeout=1)
            
            logger.info(f"[{request_id}] Processing request...")
            start_time = time.time()
            
            response = generate_response_impl(request_data)
            
            duration = time.time() - start_time
            logger.info(f"[{request_id}] Generated in {duration:.2f}s")
            
            result_container["response"] = response
            result_container["ready"] = True
            result_container["error"] = None
            
            request_queue.task_done()
            
        except Empty:
            continue
        except Exception as e:
            logger.error(f"Error processing request: {e}", exc_info=True)
            if 'result_container' in locals():
                result_container["error"] = str(e)
                result_container["ready"] = True

# Endpoints

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({
        "status": "ready" if model_loaded else "loading",
        "model_loaded": model_loaded,
        "queue_size": request_queue.qsize(),
        "version": "1.0.0",
        "timestamp": datetime.now().isoformat()
    }), 200 if model_loaded else 503

@app.route('/generate', methods=['POST'])
def generate():
    request_id = str(uuid.uuid4())[:8]
    
    if not model_loaded:
        logger.warning(f"[{request_id}] Request rejected - model not loaded")
        return jsonify({
            "response": "Just a moment... I'm still waking up!",
            "intimacy_delta": 0,
            "intimacy_score": 50,
            "intimacy_level": "Friend",
            "fallback": True
        }), 503
    
    if request_queue.full():
        logger.warning(f"[{request_id}] Request rejected - queue full")
        return jsonify({
            "response": "I'm a bit overwhelmed right now... try again in a moment!",
            "intimacy_delta": -1,
            "intimacy_score": 50,
            "intimacy_level": "Friend",
            "fallback": True
        }), 429
    
    try:
        request_data = request.get_json()
        player_input = request_data.get('player_input', '')
        personality = request_data.get('personality', 'aya')
        
        logger.info(f"[{request_id}] Request from {personality}: '{player_input}'")
        
    except Exception as e:
        logger.error(f"[{request_id}] Invalid request data: {e}")
        return jsonify({"error": "Invalid request format"}), 400
    
    result_container = {"response": None, "ready": False, "error": None}
    request_queue.put((request_id, request_data, result_container))
    
    timeout = 30
    start_wait = time.time()
    
    while not result_container["ready"] and time.time() - start_wait < timeout:
        time.sleep(0.1)
    
    if result_container["ready"]:
        if result_container["error"]:
            logger.error(f"[{request_id}] Generation failed: {result_container['error']}")
            return jsonify({
                "response": "Sorry, I got a bit confused there...",
                "intimacy_delta": 0,
                "intimacy_score": 50,
                "intimacy_level": "Friend",
                "fallback": True
            }), 500
        else:
            return jsonify(result_container["response"]), 200
    else:
        logger.error(f"[{request_id}] Request timeout")
        return jsonify({
            "response": "Sorry, I'm taking too long to think...",
            "intimacy_delta": 0,
            "intimacy_score": 50,
            "intimacy_level": "Friend",
            "fallback": True
        }), 504

@app.route('/stats', methods=['GET'])
def stats():
    return jsonify({
        "model_loaded": model_loaded,
        "queue_size": request_queue.qsize(),
        "queue_capacity": request_queue.maxsize,
        "uptime_seconds": time.time() - app.config.get('start_time', time.time())
    })

# Startup

def main():
    app.config['start_time'] = time.time()
    
    logger.info("="*60)
    logger.info("GPT-2 SERVER STARTING")
    logger.info(f"Port: {SERVER_PORT}")
    logger.info(f"Timestamp: {datetime.now():%Y-%m-%d %H:%M:%S}")
    logger.info("="*60)
    
    model_thread = threading.Thread(target=load_model, daemon=True)
    model_thread.start()
    
    processor_thread = threading.Thread(target=process_request_queue, daemon=True)
    processor_thread.start()
    
    logger.info(f"Server listening on http://localhost:{SERVER_PORT}")
    logger.info("Endpoints:")
    logger.info(f"  - GET  /health    : Health check")
    logger.info(f"  - POST /generate  : Generate dialogue")
    logger.info(f"  - GET  /stats     : Server statistics")
    logger.info("="*60)
    
    app.run(
        host='localhost',
        port=SERVER_PORT,
        debug=False,
        threaded=True,
        use_reloader=False
    )

if __name__ == '__main__':
    main()
