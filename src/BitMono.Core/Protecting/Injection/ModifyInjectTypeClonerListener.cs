﻿namespace BitMono.Core.Protecting.Injection;

public class ModifyInjectTypeClonerListener : InjectTypeClonerListener
{
    public ModifyInjectTypeClonerListener(Modifies modifies, IRenamer renamer, ModuleDefinition targetModule) : base(targetModule)
    {
        Modifies = modifies;
        Renamer = renamer;
    }

    public Modifies Modifies { get; }
    public IRenamer Renamer { get; }

    public override void OnClonedMember(IMemberDefinition original, IMemberDefinition cloned)
    {
        if (Modifies.HasFlag(Modifies.Rename))
        {
            Renamer.Rename(cloned);
        }
        if (Modifies.HasFlag(Modifies.RemoveNamespace))
        {
            Renamer.RemoveNamespace(cloned);
        }
        base.OnClonedMember(original, cloned);
    }
}