using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Protas.Control.IsolateCall
{
    public static class IsolatedInvoker
    {
        // main Invoke method
        public static void Invoke(string assemblyFile, string typeName, string methodName, object[] parameters)
        {
            // resolve path
            assemblyFile = Path.Combine(Environment.CurrentDirectory, assemblyFile);
            Debug.Assert(assemblyFile != null);

            // get base path
            var appBasePath = Path.GetDirectoryName(assemblyFile);
            Debug.Assert(appBasePath != null);

            // change current directory
            var oldDirectory = Environment.CurrentDirectory;
            Environment.CurrentDirectory = appBasePath;
            try
            {
                // create new app domain
                var domain = AppDomain.CreateDomain(Guid.NewGuid().ToString(), null, appBasePath, null, false);
                try
                {
                    // create instance
                    var invoker = (InvokerHelper) domain.CreateInstanceFromAndUnwrap(Assembly.GetExecutingAssembly().Location, typeof(InvokerHelper).FullName);

                    // invoke method
                    var result = invoker.InvokeHelper(assemblyFile, typeName, methodName, parameters);

                    // process result
                    Debug.WriteLine(result);
                }
                finally
                {
                    // unload app domain
                    AppDomain.Unload(domain);
                }
            }
            finally
            {
                // revert current directory
                Environment.CurrentDirectory = oldDirectory;
            }
        }

        // This helper class is instantiated in an isolated app domain
        private class InvokerHelper : MarshalByRefObject
        {
            // This helper function is executed in an isolated app domain
            public object InvokeHelper(string assemblyFile, string typeName, string methodName, object[] parameters)
            {
                // create an instance of the target object
                var handle = Activator.CreateInstanceFrom(assemblyFile, typeName);

                // get the instance of the target object
                var instance = handle.Unwrap();

                // get the type of the target object
                var type = instance.GetType();

                // invoke the method
                var result = type.InvokeMember(methodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, instance, parameters);

                // success
                return result;
            }
        }

        //public static class DynamicAssemblyLoader
        //{
        //    public static string ExeLoc = "";
        //    public static bool CallMethodFromDllInNewAppDomain(string exePath, string fullyQualifiedClassName, string methodName, List<object> parameters)
        //    {
        //        ExeLoc = exePath;
        //        List<Assembly> assembliesLoadedBefore = AppDomain.CurrentDomain.GetAssemblies().ToList<Assembly>();
        //        int assemblyCountBefore = assembliesLoadedBefore.Count;
        //        AppDomainSetup domaininfo = new AppDomainSetup();
        //        Evidence adevidence = AppDomain.CurrentDomain.Evidence;
        //        AppDomain domain = AppDomain.CreateDomain("testDomain", adevidence, domaininfo);
        //        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        //        domain.CreateInstanceFromAndUnwrap(exePath, fullyQualifiedClassName);
        //        List<Assembly> assemblies = domain.GetAssemblies().ToList<Assembly>();
        //        string mainExeName = System.IO.Path.GetFileNameWithoutExtension(exePath);
        //        Assembly assembly = assemblies.FirstOrDefault(c => c.FullName.StartsWith(mainExeName));
        //        Type type2 = assembly.GetType(fullyQualifiedClassName);
        //        List<Type> parameterTypes = new List<Type>();
        //        foreach (var parameter in parameters)
        //        {
        //            parameterTypes.Add(parameter.GetType());
        //        }
        //        var methodInfo = type2.GetMethod(methodName, parameterTypes.ToArray());
        //        var testClass = Activator.CreateInstance(type2);
        //        object returnValue = methodInfo.Invoke(testClass, parameters.ToArray());
        //        List<Assembly> assembliesLoadedAfter = AppDomain.CurrentDomain.GetAssemblies().ToList<Assembly>();
        //        int assemblyCountAfter = assembliesLoadedAfter.Count;
        //        if (assemblyCountAfter > assemblyCountBefore)
        //        {
        //            //  Code always comes here
        //            return false;
        //        }
        //        else
        //        {
        //            // This would prove the assembly was loaded in a NEW domain.  Never gets here.
        //            return true;
        //        }
        //    }
        //    public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        //    {
        //        // This is required I've found
        //        return System.Reflection.Assembly.LoadFrom(ExeLoc);
        //    }
        //}
    }
}
