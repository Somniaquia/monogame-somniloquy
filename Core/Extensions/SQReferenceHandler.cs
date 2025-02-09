namespace Somniloquy {
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// solution proposed by jozkee at ReferenceHandler.Preserve does not work well with JsonConverters - https://github.com/dotnet/docs/issues/21777
    /// By default reference data is only cached per call to Serialize/Deserialize causing IDs to be resetted every nested custom converters, which caused circular reference error for my TileLayer2D
    /// </summary>

    class SQReferenceHandler : ReferenceHandler {
        public SQReferenceHandler() {
            _rootedResolver = new SQReferenceResolver();
        }

        private ReferenceResolver _rootedResolver;
        public override ReferenceResolver CreateResolver() => _rootedResolver;

        class SQReferenceResolver : ReferenceResolver {
            private uint _referenceCount;
            private readonly Dictionary<string, object> _referenceIdToObjectMap = new Dictionary<string, object>();
            private readonly Dictionary<object, string> _objectToReferenceIdMap = new Dictionary<object, string>(ReferenceEqualityComparer.Instance);

            public override void AddReference(string referenceId, object value) {
                if (!_referenceIdToObjectMap.TryAdd(referenceId, value)) {
                    throw new JsonException();
                }
            }

            public override string GetReference(object value, out bool alreadyExists) {
                if (_objectToReferenceIdMap.TryGetValue(value, out string referenceId)) {
                    alreadyExists = true;
                } else {
                    _referenceCount++;
                    referenceId = _referenceCount.ToString();
                    _objectToReferenceIdMap.Add(value, referenceId);
                    alreadyExists = false;
                }

                return referenceId;
            }

            public override object ResolveReference(string referenceId) {
                if (!_referenceIdToObjectMap.TryGetValue(referenceId, out object value)) {
                    throw new JsonException();
                }

                return value;
            }
        }
    }
}