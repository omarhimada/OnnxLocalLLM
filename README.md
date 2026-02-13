# OLLM
- **Completely local** LLM chat desktop application that uses the *ONNX Generative AI Runtime*. **Does not make any networking requests outside of the local machine.**
- **Zero HTTP** *(e.g.: API calls to OpenAI, Gemini)*, **zero REST API middle-layer** *(e.g.: GPT4All)* **zero WebSocket middle-layer** *(Ollama, LM Studio, etc.)*.
- Loads a local LLM model. 
- The latest release utilizes **Phi-4** from Microsoft. I'll keep adding support for other models, as experimentation continues.

<img src="https://github.com/omarhimada/Local-LLM-ONNX/blob/master/.Images/20260212_Example.gif?raw=true" alt="Example usage of the desktop chat application." style="width: 50%; height: 50%;" />

## Roadmap 
- **High Priority**
    1. Contextual memory/conversation state management with retrieval augmentation and chat histories. **90% complete**
        - Initializes a local SQLite database if it does not exist.
  		- Utilize `VectorData` abstractions and connectors for SQLite.
      		- Microsoft is sort of developing solutions in parallel regarding native SQL Vector storage *(i.e.: `Microsoft.SemanticKernel.Connectors.SqliteVec` pre-release)*
      	-  Implemented two methods:
            1. `MemorizeDiscussion(...) // Store a discussion that had occurred.`
            2. `RememberDiscussions(...) // Try to remember before responding`
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

### Licensing and Data Sovereignty

This project is licensed under the GNU Lesser General Public License (LGPL) v2.1 to provide a transparent, high-integrity foundation for local artificial intelligence. 
Because this LLM is designed to operate entirely offline, it offers a secure alternative to centralized, cloud-based models that inadvertently require the transmission of sensitive data to third-party providers. 
The LGPL supports this "Local-First" mission by ensuring the core engine remains a public resource; any distributed improvements to the engine itself must remain open, while allowing the tool to be seamlessly integrated into private or proprietary environments without legal friction.

The choice of the LGPL is a commitment to Data Sovereignty. By ensuring the source code remains as portable as the hardware it runs on, we eliminate the "centralization risk" inherent in networked AI. 
This legal framework guarantees that the core technology cannot be revoked or turned into a "black box," providing a permanent, airgapped utility for those who require total autonomy over their information. 
Our goal is to provide a tool that is fully owned and operated by the user, ensuring that intelligence remains a private asset rather than a remote service.
