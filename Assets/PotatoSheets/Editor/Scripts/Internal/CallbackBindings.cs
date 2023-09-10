using System.Reflection;
using System;
using System.Collections.Generic;

namespace PotatoSheets.Editor {


	internal class CallbackBindings {

		private List<Binding> m_bindings;

		public CallbackBindings(ILogger logger) {
			m_bindings = new List<Binding>();
			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
				foreach (TypeInfo typeInfo in assembly.DefinedTypes) {
					Type type = typeInfo.AsType();
					ContentCallbacksAttribute attr = type.GetCustomAttribute<ContentCallbacksAttribute>();
					if (attr != null) {
						m_bindings.Add(new Binding(logger, type, attr.Order));
					}
				}
			}
			m_bindings.Sort(SortBindings);
		}

		public void OnComplete() {
			foreach (Binding binding in m_bindings) {
				binding.OnComplete();
			}
		}

		private static int SortBindings(Binding a, Binding b) {
			return a.Order.CompareTo(b.Order);
		}

		private class Binding {

			public int Order { get; }

			private MethodInfo m_onComplete;

			public Binding(ILogger logger, Type type, int order) {
				Order = order;
				m_onComplete = type.GetMethod("OnImportComplete");
				if (m_onComplete != null) {
					ParameterInfo[] parameters = m_onComplete.GetParameters();
					if ((parameters != null && parameters.Length != 0) || !m_onComplete.IsStatic) {
						logger.LogError($"ContentCallbacks type `{type.Name}' OnImportComplete method must be static and have no parameters");
					}
				}
			}

			public void OnComplete() {
				if (m_onComplete != null) {
					m_onComplete.Invoke(null, null);
				}
			}

		}

	}
}