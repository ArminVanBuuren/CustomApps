using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
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

		readonly string[] DEFAULT_REFERENCE = new string[] {
			"mscorlib.dll",
			"System.dll",
			//info.CurrentAssembly.CodeBase.Substring(8)
			//info.CurrentAssembly.ManifestModule.ScopeName
		};

		public CustomFunctions CustomFunctions { get; }

		public string Namespace { get; }

		public Type FuncType { get; }

		public Assembly CustomAssembly { get; }

		public string Code { get; }

		public Dictionary<string, T> Functions { get; }

		public CustomFunctionsCompiler(CustomFunctions customFunctions, string customNamespace = null)
		{
			CustomFunctions = customFunctions;
			Namespace = customNamespace.IsNullOrEmptyTrim() ? OUT_NAMESPACE : customNamespace;
			FuncType = typeof(T);

			Code = GenerateCode();
			CustomAssembly = ExecuteCompiling();

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

		string GenerateCode()
		{
			var securityNamespaces = $"using {FuncType.Namespace};\r\n" +
			                         @"using System.Security.Permissions;
[assembly: SecurityPermission(SecurityAction.RequestRefuse, UnmanagedCode = true)]
[assembly: FileIOPermission(SecurityAction.RequestRefuse, AllFiles = FileIOPermissionAccess.Write)]";

			var customNamespaces = string.Empty;
			if (CustomFunctions.Namespaces.Item != null && CustomFunctions.Namespaces.Item.Length > 0)
				customNamespaces = CustomFunctions.Namespaces.Item[0].Value;

			var code = new StringBuilder();
			code.Append(customNamespaces);
			code.AppendLine();
			code.AppendLine(securityNamespaces);
			code.AppendLine();
			code.Append($"namespace {Namespace}");
			code.AppendLine();
			code.Append('{');

			foreach (var func in CustomFunctions.Functions.Function)
			{
				if (func.Item == null || func.Item.Length == 0 || func.Item[0].Value.IsNullOrEmpty())
					continue;
				code.AppendLine();
				code.Append(func.Item[0].Value);
			}

			code.AppendLine();
			code.Append('}');

			return code.ToString();
		}

		private Assembly ExecuteCompiling()
		{
			var info = new AssemblyInfo(FuncType);
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

			var references = new HashSet<string>(CustomFunctions.Assemblies.Childs
				.Where(x => x.Item.Length > 0)
				.Select(x => x.Item[0].Value));
			references.UnionWith(DEFAULT_REFERENCE);

			string fileAssembly = null;
			try
			{
				var currentReference = info.CurrentAssembly.ManifestModule.Name;
				if (currentReference.Equals("<Unknown>", StringComparison.InvariantCultureIgnoreCase))
				{
					fileAssembly = CreateResourceAssembly();
					if(fileAssembly != null)
						references.Add(fileAssembly);
				}
				else
				{
					references.Add(currentReference);
				}

				foreach (var r in references)
					options.ReferencedAssemblies.Add(r);

				var compiled = provider.CompileAssemblyFromSource(options, Code);

				if (compiled.Errors.Count > 0)
				{
					var errors = new StringBuilder();
					foreach (CompilerError err in compiled.Errors)
					{
						errors.Append(err);
						errors.Append(Environment.NewLine);
					}
					throw new ExceptionCompilationError(errors.ToString());
				}
				else
				{
					return compiled.CompiledAssembly;
				}
			}
			finally
			{
				if(fileAssembly != null)
					File.Delete(fileAssembly);
			}
		}

		string CreateResourceAssembly()
		{
			foreach (var assembly in new[] { Assembly.GetEntryAssembly(), Assembly.GetCallingAssembly(), Assembly.GetExecutingAssembly()})
			{
				if (assembly == null)
					continue;

				var manifestResourceNames = assembly.GetManifestResourceNames()
					.Where(x => x.IndexOf(FuncType.Assembly.ManifestModule.ScopeName, StringComparison.InvariantCultureIgnoreCase) != -1);

				if (!manifestResourceNames.Any())
					continue;

				var tempFile = Path.GetTempFileName();
				using (var s = assembly.GetManifestResourceStream(manifestResourceNames.First()))
				using (var r = new BinaryReader(s))
				using (var fs = new FileStream(tempFile, FileMode.OpenOrCreate))
				using (var w = new BinaryWriter(fs))
					w.Write(r.ReadBytes((int)s.Length));

				//var a = Assembly.Load(data);
				//AppDomain.CurrentDomain.Load(data);
				//var ass1 = AppDomain.CurrentDomain.GetAssemblies();

				return tempFile;
			}

			return null;
		}
	}
}
