using System.Collections.Generic;
using UnityEngine;

namespace Otrabotka.Scenario.Configs
{
    [CreateAssetMenu(menuName = "Otrabotka/Configs/DialogueAsset", fileName = "DialogueAsset")]
    public class DialogueAsset : ScriptableObject
    {
        [Tooltip("Список узлов диалога для данного ассета")]
        public List<DialogueNode> Nodes = new List<DialogueNode>();
    }
} 