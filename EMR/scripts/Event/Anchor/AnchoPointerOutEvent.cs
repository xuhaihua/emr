using EMR.Entity;

namespace EMR.Event
{
    /// <summary>
    /// êhoverOut EventData��
    /// </summary>
    public class AnchorPointerOutEventData : EventData
    {
        /// <summary>
        /// ê
        /// </summary>
        public Anchor anchor;

        /// <summary>
        /// ����Ȥ���ֲ�ָ��
        /// </summary>
        public Pointer interestPointer;
    }

    /// <summary>
    /// êhoverOut�¼���
    /// </summary>
    [System.Serializable]
    public class AnchorPointerOutEvent : EMREvent<AnchorPointerOutEventData>
    {
    }
}
