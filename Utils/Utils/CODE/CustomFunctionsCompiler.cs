using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace Utils
{
	public class ExceptionCompilationError : Exception
	{
		public ExceptionCompilationError(string errors) : base($"Error of compilation custom function: {errors}") { }
	}

	public class CustomFunctionsCompiler<T>
	{
		/// <summary>
		/// This namespace wrap generated code.
		/// </summary>
		const string OUT_NAMESPACE = "CustomFunction";

		public CustomFunctions CustomFunctions { get; }

		public string Namespace { get; }

		public Assembly CustomAssembly { get; }

		public Dictionary<string, T> Functions { get; }

		public CustomFunctionsCompiler(CustomFunctions customFunctions, string customNamespace = null)
		{
			CustomFunctions = customFunctions;
			Namespace = customNamespace.IsNullOrEmptyTrim() ? OUT_NAMESPACE : customNamespace;

			var funcType = typeof(T);
			var info = new AssemblyInfo(funcType);

			var securityNamespaces = $"using {funcType.Namespace};\r\n" +
			                         @"using System.Security.Permissions;
[assembly: SecurityPermission(SecurityAction.RequestRefuse, UnmanagedCode = true)]
[assembly: FileIOPermission(SecurityAction.RequestRefuse, AllFiles = FileIOPermissionAccess.Write)]";

			var defaultReferences = new string[] {
				"mscorlib.dll",
				"System.dll",
				info.CurrentAssembly.ManifestModule.Name
			};

			var references = new HashSet<string>(customFunctions.Assemblies.Childs
				.Where(x => x.Item.Length > 0)
				.Select(x => x.Item[0].Value));
			references.UnionWith(defaultReferences);

			var customNamespaces = string.Empty;
			if (customFunctions.Namespaces.Item != null && customFunctions.Namespaces.Item.Length > 0)
				customNamespaces = customFunctions.Namespaces.Item[0].Value;

			var code = new StringBuilder();
			code.Append(customNamespaces);
			code.AppendLine();
			code.AppendLine(securityNamespaces);
			code.AppendLine();
			code.Append($"namespace {Namespace}");
			code.AppendLine();
			code.Append('{');

			foreach (var func in customFunctions.Functions.Function)
			{
				if (func.Item == null || func.Item.Length == 0 || func.Item[0].Value.IsNullOrEmpty())
					continue;
				code.AppendLine();
				code.Append(func.Item[0].Value);
			}

			code.AppendLine();
			code.Append('}');

			CustomAssembly = ExecuteCompiling(code.ToString(), references);

			Functions = new Dictionary<string, T>();
			foreach (var type in CustomAssembly.ExportedTypes)
			{
				try
				{
					var funcInstance = Activator.CreateInstance(type);
					if (funcInstance is T function)
						Functions.Add(type.Name, function);
				}
				catch (Exception ex)
				{
					throw new Exception($"Error in function invoking: \"{type.Name}\":\r\n{ex}");
				}
			}
		}

		private static Assembly ExecuteCompiling(string code, IEnumerable<string> references)
		{
			var provider = new CSharpCodeProvider();

			var options = new CompilerParameters
			{
				CompilerOptions = "/target:library /optimize+",
				GenerateExecutable = false,
				GenerateInMemory = true,
				IncludeDebugInformation = false
				//OutputAssembly = "assembly.dll";
				//ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().CodeBase.Substring(8));
			};

			foreach (var r in references)
				options.ReferencedAssemblies.Add(r);

			var compiled = provider.CompileAssemblyFromSource(options, code);

			if (compiled.Errors.Count > 0)
			{
				var errors = new StringBuilder();
				foreach (CompilerError err in compiled.Errors)
				{
					errors.Append(err.ToString());
					errors.Append(Environment.NewLine);
				}
				throw new ExceptionCompilationError(errors.ToString());
			}
			else
			{
				return compiled.CompiledAssembly;
			}
		}
	}
}
