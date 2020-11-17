using System.Runtime.CompilerServices;
using UnityEngine;

namespace Framework
{
    public class BaseEntity : MonoBehaviour
    {
        [SerializeField] private string _EntityName;

        public string EntityName
        {
            set => _EntityName = value;

            get => string.IsNullOrEmpty(_EntityName) ? name : _EntityName;
        }

        public Transform Trans { private set; get; }

        protected virtual void Awake()
        {
            Trans = transform;
        }
    }
}