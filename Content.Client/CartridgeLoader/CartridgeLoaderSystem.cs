﻿using Content.Shared.CartridgeLoader;

namespace Content.Client.CartridgeLoader;

public sealed class CartridgeLoaderSystem : SharedCartridgeLoaderSystem
{
    //Empty client system for component replication
}

[RegisterComponent]
[ComponentReference(typeof(SharedCartridgeLoaderComponent))]
public sealed class CartridgeLoaderComponent : SharedCartridgeLoaderComponent
{

}
