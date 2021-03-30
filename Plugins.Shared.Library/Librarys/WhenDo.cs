using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.Librarys
{
    public class WhenDo<T>
    {
        private bool _isValid = false;
        private bool _isValidateWhen = false;
        private bool _isExclusive = true;
        private bool _hasDone = false;

        private T _object;

        public WhenDo(T obj, bool isExclusive = true)
        {
            _object = obj;
        }

        public static WhenDo<T> New(T obj,bool isExclusive=true)
        {
            return new WhenDo<T>(obj, isExclusive);
        }

        public WhenDo<T> When(Func<T,bool> predicate)
        {
            if (_isExclusive && _hasDone)
            {
                return this;
            }

            _isValid = predicate.Invoke(_object);
            _isValidateWhen = true;
            return this;
        }

        public WhenDo<T> Do(Action<T> action)
        {
            if(_isExclusive&&_hasDone)
            {
                return this;
            }

            if(!_isValidateWhen)
            {
                throw new NotSupportedException("请先设置When条件");
            }

            if (_isValid)
            {
                action.Invoke(_object);
                _hasDone = true;
            }
            return this;
        }

        public WhenDo<T> Do(Action action)
        {
            if (_isExclusive && _hasDone)
            {
                return this;
            }

            if (!_isValidateWhen)
            {
                throw new NotSupportedException("请先设置When条件");
            }

            if (_isValid)
            {
                action.Invoke();
                _hasDone = true;
            }
            return this;
        }

        public WhenDo<T> ElseDo(Action<T> action)
        {
            if (_isExclusive && _hasDone)
            {
                return this;
            }
            action.Invoke(_object);
            _hasDone = true;
            return this;
        }
    }
}
