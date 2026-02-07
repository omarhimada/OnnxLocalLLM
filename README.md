# OLLM
- LLM local chat desktop application that uses the *ONNX Runtime Generative AI*. **Does not make any networking requests outside of the local machine.**
- Virtually zero latency due to a lack of networking communication whether it be HTTP (e.g.: API calls to OpenAI) or WebSocket middle-layer (Ollama, LM Studio, etc.).
- Loads `Mistral-3-7B` (or `Mistral-3-14B` if you can handle it with your hardware). Proceeeding with `nomic-embed-text-1-5` although exploring alternatives.

![Example Interation](/.Images/20260202-Mistral-3-14B-local-ONNX.gif)

## Roadmap 
- **High Priority**
    1. Contextual memory/conversation state management **90% complete**
        - ~~Initializes a local SQLite database if it does not exist~~
          - Complete (`memories.db` created if it does not exist locally - it will always sit beside the executable)
  		- Utilizing a `VectorData` abstractions and connectors for SQLite.
      	-  Implmented two methods:
            1. `MemorizeDiscussion(...) // Store a discussion that had occurred.`
            2. `RememberDisciossion(...) // Try to remember before responding`
          - `VectorSearch` occurs with decay parameters like `halfLifeDays = 365, etc.`
          - **The goal is that they keep learning** and you **backup the local database yourself**. *(i.e.: the model lives in this one machine and learns forever.*)
          - The model should remember what you spoke about yesterday, for example. 
        
    2. Leveraging CUDA **90% complete**
        - The suggested `DML` provided from Microsoft seems like the best route forward for utilizing CUDA in the project.
   		- ~~`Microsoft.AI.OnnxRuntime.SessionOptions` to attempt to enable GPU if available.~~
            - ~~This is expected to function easily, although yet untested. I've had no issues with other tech stacks.~~
        - Currently, due to the locality, the perceived latency between user chat input and model response is ~milliseconds.
            - This is due to the lack of the typical API request/response you find with online chat interfaces.
                - Although they're able to learn via parsing the internet, with retrieval augmentation you can also teach them without compromising proprietary information or sensitive government documentation, as an example.
                - *(Companies and governments sign contracts with OpenAI, Amazon, Meta, Google, Copilot, Cohere, etc. They don't sign contracts with the models and the databases that the models utilize to lear and recall).
                  - That is an absense of zero-trust mentality you'd expect within government organizations, R&D medical institutions, etc.
            - Also, this does not operate like LM Studio, Ollama, or Eloi, involving a secondary API and WebSocket communication between the layers on the local machine.
              - I provide zero insult, I choose a different design pattern.
            - The model is loaded into local memory on your machine and their mind and memories stay with them. This is my design I prefer.

    4. Learning with visuals/reading documents/retrieval augmentation with a local database
        - This use of a local database is quite common for similar projects although I weigh this priority less than the first two task items
          - Mistral-3-14B and other Mistral models have vision abilities, many do. This is lower priority though.

- **Medium Priority**:
        - ~~Attempt Mistral-3-14B locally instead of Mistral-3-7B due to my hardware available~~
            - ~~`RTX 5090, 32 GB DDR5 RAM, AMD Ryzen 7 9800XD` (typical desktop setup, mid-to-upper-end consumer hardware)~~
            - Watching the model's `memories.db` grow into gigabytes. Recalling and contextualizing every detail of my life for years, potentially. 
            - This seems like an important project for society, you're welcome to disagree.

- **Low Priority**
  - Other planned QOL improvements (low priority):
    - Changing models via dropdown menu selection
    - You have currently have to scroll to see their full response, it should scroll as they're responding so you read while they 'speak', so-to-speak.

### Setup
- Your directory setup should look something like the diagram below, although the `model.onnx_data` excluded. This is due to size (several gigabytes).
 - See **NVidia's ONNX Mistral-7B-Instruct** @ [HuggingFace](https://huggingface.co/nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4/tree/main) to download the `onnx_data`. (It is quite large and I cannot include it in the GitHub repository.)
- You'll have to do the same for **nomic-embed-text-1-5** @ [HuggingFace](https://huggingface.co/nomic-ai/nomic-embed-text-v1.5/tree/main/onnx) 
 - Get the ~500 MB `.onnx`, everything else is included
  - *Microsoft also has some libraries I may include for embedding to skip nomic altogether although I haven't fully investigated yet.* 
- In total you'll have to grab the `onnx_data` for Mistral-7B (or 14B) and the `.onnx` for nomic-embed-text from HF or other trusted sources. The GitHub repository limitations are unfortunate.
```
	______________________________________________________
    OnnxLocalLLM\ONNX\Mistral-7B (or Mistral-14B)
	|
	| genai_config.json
	| model.onnx
	| model.onnx_data <------------------ Download this 
	| special_tokens_map.json
	| tokenizer_config.json
	| tokenizer.json
	|____________________________________________________
	OnnxLocalLLM\ONNX\Nomic-Embed-Text-1-5
	|
	| config.json
    | config_sentence_transformers.json
	| genai_config.json
    | model.onnx  <---------------------- Download this
    | modules.json
	| special_tokens_map.json
	| tokenizer_config.json
	| tokenizer.json
    | vocab.txt
	|____________________________________________________
```

#### If you fork and customize this repository ####
1. - **I removed the `chat_template` from the `tokenizer_config.json`** due to incompatibilities during deserialization of the JSON that includes Jinja syntax.
   - **However, the logic I implemented with C# mimics the typical Jinja-esque chat logic.**

2. - You may have to do some JSON re-encoding. Some helpful tips to save you time:
   - Watch out for `UTF-8 with BOM` in the `tokenizer_json`. It will make you scratch your head for a while with odd runtime exceptions.
     - You want to make sure all the associated `*.json` is `UTF-8`. I've already done this for the JSON included in the repository (if you're cloning or downloading the latest release)
     - However if you're customizing you may have to do the same for your choice of model.
  - It is seemingly typical to download JSON that is `UTF-8 with BOM` and this leads to unexpected exceptions getting thrown during runtime.
  - You can easily convert `UTF-8 with BOM` to `UTF-8 without BOM` in most IDEs. Depending on your IDE the *save with different encoding* methodology may be different. 
  - `UTF-8 with a signature` and `UTF-8 with BOM (Byte Order Mark)` are the exact same thing. You must ensure you have neither. Only `UTF-8`.
  
  - These are some helpful PowerShell scripts if you'd like to use them for converting your `*.json` that you've downloaded elsewhere to UTF-8. 
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
