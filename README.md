# Local LLM ONNX
- Local LLM chat desktop application that uses ONNX Runtime Generative AI.
- Virtually zero latency due to a lack of HTTP (e.g.: API calls to OpenAI) and zero WebSocket middle-layer (Ollama, LM Studio, etc.).
- Does not make any networking requests outside of the local machine.
- Loads `Mistral-3` and `nomic-embed-text-1-5` locally.
![An example](/.Images/20260202-Mistral-3-14B-local-ONNX.gif)

## Roadmap
- Priorities:
    1. Contextual memory/conversation state management **80% complete**
        - Initializes a local SQLite database if it does not exist
  		- Utilizing a `VectorData` abstraction to use the SQLite database as a vector store for performance.
      	-  Implmented two methods:
            1. `MemorizeDiscussion(...) // Store a discussion that had occurred.`
            2. `RememberDisciossion(...) // Try to remember before responding`
          - `VectorSearch` occurs with decay parameters like `halfLifeDays = 365, etc.`
          - The goal is that they keep learning and you backup the local database yourself. i.e.: *the model lives in this one machine and learns forever.*
          - The model should remember what you spoke about yesterday, for example.
        
    2. Leveraging CUDA **90% complete**
        -  Utilizing `Microsoft.AI.OnnxRuntime.SessionOptions` to attempt to enable GPU if available.
            - This is expected to function easily, although yet untested. I've had no issues with other tech stacks.
            - 
        - Currently, due to the locality, the perceived latency between user chat input and model response is ~milliseconds.
            - This is due to the lack of the typical API request/response you find with online chat interfaces.
                - Although they're able to learn via parsing the internet, with retrieval augmentation you can also teach them without compromising proprietary information or sensitive government documentation.
                - *(Companies and governments sign contracts with OpenAI, Amazon, Meta, Google, and Copilot). That is an absense of zero-trust mentality you'd expect within government organizations or R&D medical laboratories, for example.*
                - 
            - Also, this does not operate like LM Studio, Ollama, or Eloi, involving a secondary API and WebSocket communication between the layers on the local machine.
            - Thte model is loaded into memory and is localized on your machine. This is quite distinct.        	

    3. Learning with visuals/reading documents/retrieval augmentation with a local database
        - This use of a local database is quite common for similar projects although I weigh this priority less than the first two task items

- Plan:
    - After priorities are met:
        - Attempt Mistral-3-14B locally instead of Mistral-3-7B due to my hardware available 
            - `RTX 5090, 32 GB DDR5 RAM, AMD Ryzen 7 9800XD` (typical desktop setup, mid-to-upper-end consumer hardware)
            - I plan on watching the model's `memories.db` grow into gigabytes. Recalling and contextualizing every detail of my life for years.
            - This seems important to me. You're welcome to disagree.

- Other planned QOL improvements:
    - Changing models via dropdown menu selection
    - You have currently have to scroll to see their full response, it should scroll as they're responding so you read while they 'speak', so-to-speak.

## Setup
- Your directory setup should look something like the diagram below, although the `model.onnx_data` excluded. This is due to size (several gigabytes).
 - See **NVidia's ONNX Mistral-7B-Instruct** @ [HuggingFace](https://huggingface.co/nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4/tree/main) to download them both.
- You'll have to do the same for **nomic-embed-text-1-5** @ [HuggingFace](https://huggingface.co/nomic-ai/nomic-embed-text-v1.5/tree/main/onnx) 
 - Get the ~500 MB `.onnx`, everything else is included 
- In total you'll have to grab the onnx_data for Mistral and the Nomic onnx for the embedding from HF. The GitHub repository limitations are unfortunate.
```
    .OnnxLocalLLM\Mistral-7B______________________________
	|
	| genai_config.json
	| model.onnx
	| model.onnx_data <------------------ Download this
	| special_tokens_map.json
	| tokenizer_config.json
	| tokenizer.json
	|____________________________________________________
	|.OnnxLocalLLM\Nomic-Embed-Text-1-5
	|
	| config.json
    | config_sentence_transformers.json
	| genai_config.json
    | model.onnx  <---------------------- Download this.
    | modules.json
	| special_tokens_map.json
	| tokenizer_config.json
	| tokenizer.json
    | vocab.txt
	|____________________________________________________
```

### Cloning/forking and changing ONNX models
- The ONNX directory structure is important. 
- As soon as the application begins to load it looks for the all of the expected JSON including the ONNX and associated data within that directory.
- See `App.xaml.cs`
```csharp
Config config = new(DebugModelPath);
Model model = new(config);
Tokenizer tokenizer = new(model);

using OnnxRuntimeGenAIChatClient onnxChatClient = new(model);
```
#### Encoding ONNX-related JSON
- It is seemingly typical to download JSON that is UTF-8 with BOM and this leads to unexpected exceptions getting thrown during runtime.
- The JSON included in this repository has already been converted from `UTF-8 (with a signature)` to `UTF-8 (without a signature)`
- You can easily convert UTF-8 with BOM to UTF-8 without BOM in most IDEs. Depending on your IDE the *save with different encoding* methodology may be different. 
- `UTF-8 with a signature` and `UTF-8 with BOM (Byte Order Mark)` are the exact same thing. You must ensure you have neither. Only `UTF-8` for the ONNX JSON.
- I also removed the `chat_template` from the `tokenizer_config.json` due to Jinja incompatibilities. **The logic I implemented with C# essentially mimics the Jinja logic, though.**

- - There are some helpful PowerShell scripts if you'd prefer to use them instead:
```powershell
$cfg = "C:\...\OnnxLocalLLM\Mistral-7B\genai_config.json"
$text = Get-Content $cfg -Raw
[IO.File]::WriteAllText($cfg, $text, (New-Object System.Text.UTF8Encoding($false)))
```

```powershell
$cfg = "C:\...\OnnxLocalLLM\Mistral-7B\special_tokens_map.json"
>> $text = Get-Content $cfg -Raw
>> [IO.File]::WriteAllText($cfg, $text, (New-Object System.Text.UTF8Encoding($false)))
```

```powershell
$cfg = "C:\...\OnnxLocalLLM\Mistral-7B\tokenizer_config.json"
$text = Get-Content $cfg -Raw
[IO.File]::WriteAllText($cfg, $text, (New-Object System.Text.UTF8Encoding($false)))
```

```powershell
$cfg = "C:\...\OnnxLocalLLM\Mistral-7B\tokenizer.json"
$text = Get-Content $cfg -Raw
[IO.File]::WriteAllText($cfg, $text, (New-Object System.Text.UTF8Encoding($false)))
```
