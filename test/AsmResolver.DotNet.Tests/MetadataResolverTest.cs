using System;
using System.IO;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.NestedClasses;
using AsmResolver.PE.DotNet.Cil;
using Xunit;

namespace AsmResolver.DotNet.Tests
{
    public class MetadataResolverTest
    {
        private readonly DefaultMetadataResolver _fwResolver;
        private readonly DefaultMetadataResolver _coreResolver;
        private readonly SignatureComparer _comparer;
        
        public MetadataResolverTest()
        {
            _fwResolver = new DefaultMetadataResolver(new NetFrameworkAssemblyResolver()
            {
                SearchDirectories =
                {
                    Path.GetDirectoryName(typeof(MetadataResolverTest).Assembly.Location)
                }
            });
            _coreResolver = new DefaultMetadataResolver(new NetCoreAssemblyResolver()
            {
                SearchDirectories =
                {
                    Path.GetDirectoryName(typeof(MetadataResolverTest).Assembly.Location)
                }
            });
            _comparer = new SignatureComparer();
        }
        
        [Fact]
        public void ResolveSystemObjectFramework()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);

            var reference = new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "Object");
            var definition = _fwResolver.ResolveType(reference);

            Assert.Equal((ITypeDefOrRef) reference, definition, _comparer);
        }
        
        [Fact]
        public void ResolveSystemObjectNetCore()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld_NetCore);

            var reference = new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "Object");
            var definition = _coreResolver.ResolveType(reference);

            Assert.True(definition.IsTypeOf(reference.Namespace, reference.Name));
        }

        [Fact]
        public void ResolveCorLibTypeSignature()
        {
            var module = ModuleDefinition.FromBytes(Properties.Resources.HelloWorld);
            var definition = _fwResolver.ResolveType(module.CorLibTypeFactory.Object);
            
            Assert.Equal(module.CorLibTypeFactory.Object.Type, definition, _comparer);
        }
        
        [Fact]
        public void ResolveType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);

            var topLevelClass1 = new TypeReference(new AssemblyReference(module.Assembly),
                typeof(TopLevelClass1).Namespace, typeof(TopLevelClass1).Name);
            
            var definition = _coreResolver.ResolveType(topLevelClass1);
            Assert.Equal((ITypeDefOrRef) topLevelClass1, definition, _comparer);
        }

        [Fact]
        public void ResolveTypeReferenceTwice()
        {
            var module = new ModuleDefinition("SomeModule.dll");

            var consoleType = new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "Console");
            Assert.Same(_fwResolver.ResolveType(consoleType), _fwResolver.ResolveType(consoleType));
        }

        [Fact]
        public void ResolveTypeReferenceThenChangeRefAndResolveAgain()
        {
            var module = new ModuleDefinition("SomeModule.dll");

            ITypeDefOrRef expected = new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "Object");
            var reference = new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "Object");
            Assert.Equal(expected, _fwResolver.ResolveType(reference), _comparer);
            reference.Name = "String";
            Assert.NotEqual(expected, _fwResolver.ResolveType(reference), _comparer);
        }

        [Fact]
        public void ResolveTypeReferenceThenChangeDefAndResolveAgain()
        {
            var module = new ModuleDefinition("SomeModule.dll");

            ITypeDefOrRef expected = new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "Object");
            var reference = new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "Object");
            var definition = _fwResolver.ResolveType(reference);
            Assert.Equal(expected, definition, _comparer);
            definition.Name = "String";
            Assert.NotEqual(expected, _fwResolver.ResolveType(reference), _comparer);
        }
        
        [Fact]
        public void ResolveNestedType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);

            var topLevelClass1 = new TypeReference(new AssemblyReference(module.Assembly),
                typeof(TopLevelClass1).Namespace, typeof(TopLevelClass1).Name);
            var nested1 = new TypeReference(topLevelClass1,null, typeof(TopLevelClass1.Nested1).Name);
            
            var definition = _coreResolver.ResolveType(nested1);
            
            Assert.Equal((ITypeDefOrRef) nested1, definition, _comparer);
        }
        
        [Fact]
        public void ResolveNestedNestedType()
        {
            var module = ModuleDefinition.FromFile(typeof(TopLevelClass1).Assembly.Location);

            var topLevelClass1 = new TypeReference(new AssemblyReference(module.Assembly),
                typeof(TopLevelClass1).Namespace, typeof(TopLevelClass1).Name);
            var nested1 = new TypeReference(topLevelClass1,null, typeof(TopLevelClass1.Nested1).Name);
            var nested1nested1 = new TypeReference(nested1,null, typeof(TopLevelClass1.Nested1.Nested1Nested1).Name);
            
            var definition = _fwResolver.ResolveType(nested1nested1);
            
            Assert.Equal((ITypeDefOrRef) nested1nested1, definition, _comparer);
        }

        [Fact]
        public void ResolveConsoleWriteLineMethod()
        {
            var module = new ModuleDefinition("SomeModule.dll");

            var consoleType = new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "Console");
            var writeLineMethod = new MemberReference(consoleType, "WriteLine",
                MethodSignature.CreateStatic(module.CorLibTypeFactory.Void, module.CorLibTypeFactory.String));

            var definition = _fwResolver.ResolveMethod(writeLineMethod);

            Assert.NotNull(definition);
            Assert.Equal(writeLineMethod.Name, definition.Name);
            Assert.Equal(writeLineMethod.Signature, definition.Signature, _comparer);
        }

        [Fact]
        public void ResolveStringEmptyField()
        {
            var module = new ModuleDefinition("SomeModule.dll");

            var stringType = new TypeReference(module.CorLibTypeFactory.CorLibScope, "System", "String");
            var emptyField = new MemberReference(stringType, "Empty",
                FieldSignature.CreateStatic(module.CorLibTypeFactory.String));

            var definition = _fwResolver.ResolveField(emptyField);

            Assert.NotNull(definition);
            Assert.Equal(emptyField.Name, definition.Name);
            Assert.Equal(emptyField.Signature, definition.Signature, _comparer);
        }
    }
}