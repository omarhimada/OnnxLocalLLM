# OLLM
- **Completely local** LLM chat desktop application that uses the *ONNX Generative AI Runtime*. **Does not make any networking requests outside of the local machine.**
- **Zero HTTP** *(e.g.: API calls to OpenAI, Gemini)*, **zero REST API middle-layer** *(e.g.: GPT4All)* **zero WebSocket middle-layer** *(Ollama, LM Studio, etc.)*.
- Loads a local LLM model. 
- The latest release utilizes **Phi-4** from Microsoft. I'll keep adding support for other models, as experimentation continues.

![Example Interation](/.Images/20260202-Mistral-3-14B-local-ONNX.gif)

## Roadmap 
- **High Priority**
    1. Contextual memory/conversation state management with retrieval augmentation and chat histories. **90% complete**
        - Initializes a local SQLite database if it does not exist.
  		- Utilize `VectorData` abstractions and connectors for SQLite.
      		- Microsoft is sort of developing solutions in parallel regarding native SQL Vector storage *(i.e.: `Microsoft.SemanticKernel.Connectors.SqliteVec` pre-release)*
      	-  Implemented two methods:
            1. `MemorizeDiscussion(...) // Store a discussion that had occurred.`
            2. `RememberDiscussion(...) // Try to remember before responding`
          - `VectorSearch` occurs with decay parameters like `halfLifeDays = 365, etc.`
          - **The goal is that they keep learning** and you **backup the local database yourself**. *(i.e.: the model lives in this one machine and learns forever.*)
        
- **Low Priority**
  - Other planned QOL improvements (low priority):
    - Image/vision -> embeddings -> retrieval augmentation. I don't want to fast-forward this with existing solutions.
    - Changing models via dropdown menu selection

### Setup
- Your directory setup should look something like the diagram below, although the `model.onnx` and `model.onnx_data` will be absent. This is due to size (gigabytes).
 - See [**Microsoft's Phi-4** @ huggingface](https://huggingface.co/microsoft/phi-4-onnx/tree/main/gpu/gpu-int4-rtn-block-32) to download the `model.onnx` and `model.onnx_data`.
```
        ,______________________________________________________
        | OnnxLocalLLM\ONNX\Phi-4
        |
        | model.onnx        <------------------ Download this 
        | model.onnx_data   <------------------ Download this 
        |
        | genai_config.json
        | special_tokens_map.json
        | tokenizer_config.json
        | tokenizer.json
        | vocab.json
        |____________________________________________________
```