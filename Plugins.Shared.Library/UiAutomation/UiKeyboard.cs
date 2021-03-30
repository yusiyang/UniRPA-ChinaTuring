using FlaUI.Core.Input;
using FlaUI.Core.WindowsAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugins.Shared.Library.UiAutomation
{
    public class UiKeyboard:IDisposable
    {
        private Queue<VirtualKey> virtualKeyQueue = new Queue<VirtualKey>();

        public static UiKeyboard New()
        {
            return new UiKeyboard();
        }

        public UiKeyboard Press(VirtualKey virtualKey)
        {
            Keyboard.Press((VirtualKeyShort)virtualKey);
            virtualKeyQueue.Enqueue(virtualKey);

            return this;
        }

        public UiKeyboard With(VirtualKey virtualKey)
        {
            Keyboard.Press((VirtualKeyShort)virtualKey);
            virtualKeyQueue.Enqueue(virtualKey);

            return this;
        }

        public UiKeyboard Continue(VirtualKey virtualKey)
        {
            VirtualKey key;
            while(virtualKeyQueue.Count>0)
            {
                key = virtualKeyQueue.Dequeue();
                Keyboard.Release((VirtualKeyShort)key);
            }

            Keyboard.Press((VirtualKeyShort)virtualKey);
            virtualKeyQueue.Enqueue(virtualKey);

            return this;

        }

        public void End()
        {
            VirtualKey key;
            while (virtualKeyQueue.Count > 0)
            {
                key = virtualKeyQueue.Dequeue();
                Keyboard.Release((VirtualKeyShort)key);
            }
        }

        public void Dispose()
        {
            End();
        }
    }
}
