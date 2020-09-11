using Thunder.Utility;
using UnityEngine;

namespace Thunder.Entity
{
    public class BaseEntity : MonoBehaviour
    {
        public string EntityName
        {
            set => _EntityName = value;

            get => string.IsNullOrEmpty(_EntityName) ? name : _EntityName;
        }

        [DontInject]
        [SerializeField]
        private string _EntityName;

        protected Transform _Trans;

        protected virtual void Awake()
        {
            _Trans = transform;
        }
    }
}
