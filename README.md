# Local LLM ONNX
- Local LLM chat desktop application that uses ONNX Runtime Generative AI.
- Does not make any networking requests outside of the local machine.

![Screenshot of the chat interface as it begins to load](LoadingScreenshot.png)

- After ~2-3 seconds:

![Screenshot of the chat interface](voila.png)

## Roadmap
- Priorities:
    1. Contextual memory/conversation state management
        - The model should remember what you spoke about yesterday, for example.
        - Ensure this is done locally beside the model on the local machine.
            - SQLite is one possibility.
    2. Leveraging CUDA 
        - Currently due to the locality the perceived lag between simple chat responses (with a small enough model i.e.: **Mini**stral 3) is milliseconds.
            - This is due to the lack of the typical API request/response you find with online chat interfaces
            - Also, this does not operate like LM Studio, Ollama, or Eloi, involving a secondary API and WebSocket communication between the layers on the local machine.
        	- **(i.e.: the complete locality while ensuring UI updates do not occur on the UI thread of the system seems to be more than sufficient for a responsive, healthy user experience)**
    3. Learning with visuals/reading documents/retrieval augmentation with a local database
        - This use of a local database is quite common for similar projects although I weigh this priority less than the first two task items

- QOL improvements:
    - Changing models via dropdown menu selection
    - You have currently have to scroll to see their full response, it should scroll as they're responding so you read while they 'speak', so-to-speak.

## Setup
- Your directory setup should look something like the diagram below, although the `model.onnx` and `model.onnx_data` excluded. This is due to size (~4 GB).
 - See **NVidia's ONNX Mistral-7B-Instruct** @ [HuggingFace](https://huggingface.co/nvidia/Mistral-7B-Instruct-v0.3-ONNX-INT4/tree/main) to download them both.
 - Download the `model.onnx_data` and place it inside the `\Mistral-B` directory.
```
OnnxLocalLLM\Mistral-7B
			|
			| genai_config.json
			| model.onnx
			| model.onnx_data
			| special_tokens_map.json
			| tokenizer_config.json
			| tokenizer.json
			|_____________________
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

`dotnet add package Microsoft.SemanticKernel.Connectors.SqliteVec --version 1.70.0-preview`
