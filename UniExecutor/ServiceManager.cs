using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniExecutor
{
	public class ServiceManager : IServiceProvider, IEnumerable<Type>, IEnumerable
	{
		private class PublishProxy<TServiceType>
		{
			private PublishServiceCallback<TServiceType> _genericCallback;

			internal PublishServiceCallback Callback => PublishService;

			internal PublishProxy(PublishServiceCallback<TServiceType> callback)
			{
				_genericCallback = callback;
			}

			private object PublishService(Type serviceType)
			{
				if (serviceType == null)
				{
					throw new ArgumentNullException("serviceType");
				}
				if (serviceType != typeof(TServiceType))
				{
					throw new InvalidOperationException();
				}
				object obj = _genericCallback();
				if (obj == null)
				{
					throw new InvalidOperationException();
				}
				if (!serviceType.IsInstanceOfType(obj))
				{
					throw new InvalidOperationException();
				}
				return obj;
			}
		}

		private class SubscribeProxy<TServiceType> : ICallbackProxy
		{
			private SubscribeServiceCallback<TServiceType> _genericCallback;

			internal SubscribeServiceCallback Callback => SubscribeService;

			Delegate ICallbackProxy.OriginalDelegate => _genericCallback;

			object ICallbackProxy.OriginalTarget => _genericCallback.Target;

			internal SubscribeProxy(SubscribeServiceCallback<TServiceType> callback)
			{
				_genericCallback = callback;
			}

			private void SubscribeService(Type serviceType, object service)
			{
				if (serviceType == null)
				{
					throw new ArgumentNullException("serviceType");
				}
				if (service == null)
				{
					throw new ArgumentNullException("service");
				}
				if (!typeof(TServiceType).IsInstanceOfType(service))
				{
					throw new InvalidOperationException();
				}
				_genericCallback((TServiceType)service);
			}
		}

		private interface ICallbackProxy
		{
			Delegate OriginalDelegate
			{
				get;
			}

			object OriginalTarget
			{
				get;
			}
		}

		private static readonly object _recursionSentinel = new object();

		private Dictionary<Type, object> _services;

		private Dictionary<Type, SubscribeServiceCallback> _subscriptions;

		public bool Contains(Type serviceType)
		{
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (_services != null)
			{
				return _services.ContainsKey(serviceType);
			}
			return false;
		}

		public bool Contains<TServiceType>()
		{
			return Contains(typeof(TServiceType));
		}

		public TServiceType GetRequiredService<TServiceType>()
		{
			TServiceType service = GetService<TServiceType>();
			if (service == null)
			{
				throw new NotSupportedException();
			}
			return service;
		}

		public TServiceType GetService<TServiceType>()
		{
			object service = GetService(typeof(TServiceType));
			return (TServiceType)service;
		}

		public object GetService(Type serviceType)
		{
			object obj = GetPublishedService(serviceType);
			if (obj == null && Contains(typeof(IServiceProvider)))
			{
				obj = GetRequiredService<IServiceProvider>().GetService(serviceType);
				if (obj != null)
				{
					Publish(serviceType, obj);
				}
			}
			return obj;
		}

		private object GetPublishedService(Type serviceType)
		{
			object value = null;
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (_services != null && _services.TryGetValue(serviceType, out value))
			{
				if (value == _recursionSentinel)
				{
					throw new InvalidOperationException();
				}
				PublishServiceCallback publishServiceCallback = value as PublishServiceCallback;
				if (publishServiceCallback != null)
				{
					_services[serviceType] = _recursionSentinel;
					try
					{
						value = publishServiceCallback(serviceType);
						if (value == null)
						{
							throw new InvalidOperationException();
						}
						if (!serviceType.IsInstanceOfType(value))
						{
							throw new InvalidOperationException();
						}
						return value;
					}
					finally
					{
						_services[serviceType] = value;
					}
				}
			}
			return value;
		}

		public IEnumerator<Type> GetEnumerator()
		{
			if (_services == null)
			{
				_services = new Dictionary<Type, object>();
			}
			return _services.Keys.GetEnumerator();
		}

		public void Subscribe(Type serviceType, SubscribeServiceCallback callback)
		{
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			object service = GetService(serviceType);
			if (service != null)
			{
				callback(serviceType, service);
				return;
			}
			if (_subscriptions == null)
			{
				_subscriptions = new Dictionary<Type, SubscribeServiceCallback>();
			}
			SubscribeServiceCallback value = null;
			_subscriptions.TryGetValue(serviceType, out value);
			value = (SubscribeServiceCallback)Delegate.Combine(value, callback);
			_subscriptions[serviceType] = value;
		}

		public void Subscribe<TServiceType>(SubscribeServiceCallback<TServiceType> callback)
		{
			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			SubscribeProxy<TServiceType> subscribeProxy = new SubscribeProxy<TServiceType>(callback);
			Subscribe(typeof(TServiceType), subscribeProxy.Callback);
		}

		public void Publish(Type serviceType, PublishServiceCallback callback)
		{
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			Publish(serviceType, (object)callback);
		}

		public void Publish(Type serviceType, object serviceInstance)
		{
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (serviceInstance == null)
			{
				throw new ArgumentNullException("serviceInstance");
			}
			if (!(serviceInstance is PublishServiceCallback) && !serviceType.IsInstanceOfType(serviceInstance))
			{
				throw new ArgumentException();
			}
			if (_services == null)
			{
				_services = new Dictionary<Type, object>();
			}
			try
			{
				_services.Add(serviceType, serviceInstance);
			}
			catch (ArgumentException innerException)
			{
				throw new ArgumentException();
			}
			if (_subscriptions != null && _subscriptions.TryGetValue(serviceType, out SubscribeServiceCallback value))
			{
				value(serviceType, GetService(serviceType));
				_subscriptions.Remove(serviceType);
			}
		}

		public void Publish<TServiceType>(PublishServiceCallback<TServiceType> callback)
		{
			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			PublishProxy<TServiceType> publishProxy = new PublishProxy<TServiceType>(callback);
			Publish(typeof(TServiceType), publishProxy.Callback);
		}

		public void Publish<TServiceType>(TServiceType serviceInstance)
		{
			if (serviceInstance == null)
			{
				throw new ArgumentNullException("serviceInstance");
			}
			Publish(typeof(TServiceType), serviceInstance);
		}

		public void Unsubscribe<TServiceType>(SubscribeServiceCallback<TServiceType> callback)
		{
			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			SubscribeProxy<TServiceType> subscribeProxy = new SubscribeProxy<TServiceType>(callback);
			Unsubscribe(typeof(TServiceType), subscribeProxy.Callback);
		}

		public void Unsubscribe(Type serviceType, SubscribeServiceCallback callback)
		{
			if (serviceType == null)
			{
				throw new ArgumentNullException("serviceType");
			}
			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			if (_subscriptions != null && _subscriptions.TryGetValue(serviceType, out SubscribeServiceCallback value))
			{
				value = (SubscribeServiceCallback)ServiceManager.RemoveCallback(value, callback);
				if (value == null)
				{
					_subscriptions.Remove(serviceType);
				}
				else
				{
					_subscriptions[serviceType] = value;
				}
			}
		}

		protected static object GetTarget(Delegate callback)
		{
			if ((object)callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			ICallbackProxy callbackProxy = callback.Target as ICallbackProxy;
			if (callbackProxy != null)
			{
				return callbackProxy.OriginalTarget;
			}
			return callback.Target;
		}

		protected static Delegate RemoveCallback(Delegate existing, Delegate toRemove)
		{
			if ((object)existing == null)
			{
				return null;
			}
			if ((object)toRemove == null)
			{
				return existing;
			}
			ICallbackProxy callbackProxy = toRemove.Target as ICallbackProxy;
			if (callbackProxy == null)
			{
				return Delegate.Remove(existing, toRemove);
			}
			toRemove = callbackProxy.OriginalDelegate;
			Delegate[] invocationList = existing.GetInvocationList();
			bool flag = false;
			for (int i = 0; i < invocationList.Length; i++)
			{
				Delegate @delegate = invocationList[i];
				ICallbackProxy callbackProxy2 = @delegate.Target as ICallbackProxy;
				if (callbackProxy2 != null)
				{
					@delegate = callbackProxy2.OriginalDelegate;
				}
				if (@delegate.Equals(toRemove))
				{
					invocationList[i] = null;
					flag = true;
				}
			}
			if (flag)
			{
				existing = null;
				Delegate[] array = invocationList;
				foreach (Delegate delegate2 in array)
				{
					if ((object)delegate2 != null)
					{
						existing = (((object)existing != null) ? Delegate.Combine(existing, delegate2) : delegate2);
					}
				}
			}
			return existing;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
