using System;

namespace Facens.Test
{
    [Serializable]
    public sealed class TestProcedureDefinition
    {
        public string id;
        public string title;
        public TestProcedureStep[] steps;
    }

    [Serializable]
    public sealed class TestProcedureStep
    {
        public int id;
        public string component;
        public string title;
        public string instruction;
    }

    public sealed class TestComponentTag : UnityEngine.MonoBehaviour
    {
        public string componentId;
    }
}
