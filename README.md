# Sanctuary
A Dynamic Save System For Unity

# System Flow Chart
```mermaid
stateDiagram-v2
    o0 : Objects
    o0 : Monobehaviours
    o0 : Scriptable Objects

    o1 : Registration
    o1 : Save Scope (Enum)  
    o1 : Save Location (Object IDs)
    o1 : Save Data (System.object)  

    s0: Save Store Registry
    s0: Reference Management
    s0: Save / Load Distribution

    s1 : Save Provider
    s1 : SaveControllerBase (Class)
    s1 : FileSaveLoader (Class)
    s1 : ISerializer (Interface)

    s2 : Save Scopes
    s2 : Absolute (Singleton)
    s2 : Global (Singleton | Indexed)
    s2 : Scene (Indexed | Local)
    s2 : Temporary

    o2 : Distribution
    o2 : Profile Data (Index | Scope)

    o2.1 : Monobehaviours
    o2.2 : Scriptable Objects
    o2.3 : Other Objects Types

    o0 --> o1
    o1 --> s0  
    s0 --> s1       
    s1 --> s2
    s0 --> o2
    s2 --> o2

    o2 --> o2.1
    o2 --> o2.2
    o2 --> o2.3
```

# To Do: 
- [X] Finish ISerializer Interfaces
- [ ] Simplify Set Up:
    - [ ] Change Save Provider A To Static Class
    - [ ] Merge SaveControllerBase & Save Scope Based Classes
    - [ ] Offload Some FileSaveLoader Methods To Extension Methods
- [ ] Reimplement Editor Creation / Loading
- [ ] Add Save/Load Property Attributes Set Up Like Dependency Injection
- [ ] Add Stress Tests For Error Handling & Benchmarking
- [ ] Editor Tab For Locating Registered Scripts