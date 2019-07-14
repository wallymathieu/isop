using System;

namespace Tests.FakeControllers
{
    public class DisposeController:IDisposable
    {
        public DisposeController()
        {
        }

        public event Action OnDispose;
        public static event Action<DisposeController> StaticOnDispose;

        public string Method()
        {
            return "test";
        }

        public void Dispose()
        {
            if (OnDispose!=null) OnDispose();
            if (StaticOnDispose!=null) StaticOnDispose(this);
        }
    }
}

