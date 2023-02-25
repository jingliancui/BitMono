﻿global using AsmResolver;
global using AsmResolver.DotNet;
global using AsmResolver.DotNet.Cloning;
global using AsmResolver.DotNet.Signatures;
global using AsmResolver.PE.DotNet.Cil;
global using BitMono.API.Protecting;
global using BitMono.API.Protecting.Analyzing;
global using BitMono.API.Protecting.Contexts;
global using BitMono.API.Protecting.Pipeline;
global using BitMono.API.Protecting.Renaming;
global using BitMono.API.Protecting.Resolvers;
global using BitMono.Core.Extensions;
global using BitMono.Core.Protecting.Analyzing;
global using BitMono.Core.Protecting.Attributes;
global using BitMono.Core.Protecting.Injection;
global using BitMono.Core.Protecting.Resolvers;
global using BitMono.Shared.Models;
global using BitMono.Utilities.Extensions.AsmResolver;
global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Threading.Tasks;
global using AsmResolver.DotNet.Code.Cil;
global using BitMono.Core.Configuration;
global using Echo.DataFlow.Analysis;
global using Echo.Platforms.AsmResolver;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Configuration.Json;
global using Microsoft.Extensions.Options;
global using Newtonsoft.Json;
global using Pocket.Extensions;
global using FieldAttributes = AsmResolver.PE.DotNet.Metadata.Tables.Rows.FieldAttributes;
global using TypeAttributes = AsmResolver.PE.DotNet.Metadata.Tables.Rows.TypeAttributes;