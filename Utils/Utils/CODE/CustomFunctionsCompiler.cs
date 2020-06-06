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

	public interface ICustomFunction
	{
		string Invoke(string[] args);
	}

	public class CustomFunctionsCompiler
	{
		/// <summary>
		/// This namespace wrap generated code.
		/// </summary>
		const string OUT_NAMESPACE = "CustomFunction";

		/// <summary>
		/// Attributes restricting
		/// rights of compiled code. 
		/// </summary>
		private static readonly string SECURITY_ATTR;

		/// <summary>
		/// References on assembly
		/// added on default.
		/// </summary>
		private static readonly string[] REFERENCES;

		public string Namespace { get; }

		public Assembly CustomAssembly { get; }

		public Dictionary<string, ICustomFunction> Functions { get; }

		static CustomFunctionsCompiler()
		{
			var info = new AssemblyInfo(typeof(ICustomFunction));
			
			SECURITY_ATTR = $"using {typeof(ICustomFunction).Namespace};\r\n" +
							@"using System.Security.Permissions;
[assembly: SecurityPermission(SecurityAction.RequestRefuse, UnmanagedCode = true)]
[assembly: FileIOPermission(SecurityAction.RequestRefuse, AllFiles = FileIOPermissionAccess.Write)]";
			
			REFERENCES = new string[] {
				"mscorlib.dll",
				"System.dll",
				info.CurrentAssembly.ManifestModule.Name
			};
		}

		public CustomFunctionsCompiler(CustomFunctions customFunctions, string customNamespace = null)
		{
			Namespace = customNamespace.IsNullOrEmptyTrim() ? OUT_NAMESPACE : customNamespace;
			var references = new HashSet<string>(customFunctions.Assemblies.Childs
				.Where(x => x.Item.Length > 0)
				.Select(x => x.Item[0].Value));
			references.UnionWith(REFERENCES);

			var customNamespaces = string.Empty;
			if (customFunctions.Namespaces.Item != null && customFunctions.Namespaces.Item.Length > 0)
				customNamespaces = customFunctions.Namespaces.Item[0].Value;

			var code = new StringBuilder();
			code.Append(customNamespaces);
			code.AppendLine();
			code.AppendLine(SECURITY_ATTR);
			code.AppendLine();
			code.Append($"namespace {OUT_NAMESPACE}");
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

			Functions = new Dictionary<string, ICustomFunction>();
			foreach (var type in CustomAssembly.ExportedTypes)
			{
				try
				{
					var funcInstance = Activator.CreateInstance(type);
					if (funcInstance is ICustomFunction function)
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
