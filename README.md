# OLLM
- **Completely local** LLM chat desktop application that uses the *ONNX Generative AI Runtime*. **Does not make any networking requests outside of the local machine.**
- **Zero HTTP** *(e.g.: API calls to OpenAI, Gemini)*, **zero REST API middle-layer** *(e.g.: GPT4All)* **zero WebSocket middle-layer** *(Ollama, LM Studio, etc.)*.
- Loads a local LLM model. 
- The latest release utilizes **Phi-4** from Microsoft. I'll keep adding support for other models, as experimentation continues.

![Example](/.Images/20260212_Example.gif)

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
#### Licensing Philosophy

This project is licensed under the GNU General Public License (GPL) to ensure that the software remains a transparent and permanent resource for the public. 
Because this LLM is designed to operate entirely offline and adapt to its user’s needs, it represents a fundamental shift toward individual digital autonomy. 
The GPL supports this by requiring that any distributed improvements or modifications to the core engine are shared with the same level of transparency. 
This creates a "continuous audit" environment where the safety and logic of the system can be verified by anyone, preventing the core technology from being obscured or restricted.
The choice of a copyleft license is a commitment to the long-term accessibility of local AI. By ensuring that the source code remains as portable and open as the hardware it runs on, 
we prevent the "centralization risk" that occurs when critical tools are moved behind closed, networked barriers. Our goal is to provide a tool that is fully owned by the person operating it; the GPL acts as the legal framework that guarantees this local-first architecture cannot be revoked or turned into a "black box" by subsequent distributors.
Would you like me to help you draft the "Safety & Usage" section to address the risks of teaching the model exploits?
