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
        
    2. Leveraging CUDA **100% complete** - fallback logic involving DirectML was creating collisions making it impossible to use either. This seems to be a bug that may be corrected in the future.
        - Currently, due to the locality, the perceived latency between user chat input and model response is ~~~milliseconds~~.
            - **Phi-4's vocabulary is so large that there is *some* latency - it isn't as responsive as before. However, the result is higher quality.**
            - This is due to the lack of the typical API request/response you find with online chat interfaces.
                - Although the popular solutions are able to learn via parsing the internet (see ChatGPT and Gemini), you will inevitably teach them compromising proprietary or sensitive information, as an example.
                - **(You can't sign a contract with an LLM to forget proprietary information, but you can sign the papers that the lawyer hands you.)**
                  - That is an absence of zero-trust mentality you'd expect within government organizations, R&D medical institutions, etc.
            - Also, this does not operate like LM Studio, Ollama, or Eloi, involving a secondary API and WebSocket communication between the layers on the local machine.
              - I provide zero insult, I choose a different design pattern.
            - The model is loaded into local memory on your machine and their mind and memories stay with them. This is my design I prefer.

- **Low Priority**
  - Other planned QOL improvements (low priority):
    - Image/vision -> embeddings -> retrieval augmentation. I don't want to fast-forward this with existing solutions.
    - Changing models via dropdown menu selection
    - You currently have to scroll to see their full response, it should scroll as they're responding so you read while they 'speak', so-to-speak.

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