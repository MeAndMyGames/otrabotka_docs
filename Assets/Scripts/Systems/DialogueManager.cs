using System;
using System.Collections.Generic;
using UnityEngine;
using Otrabotka.Scenario.Configs;
using Otrabotka.UI;

namespace Otrabotka.Systems
{
    [DisallowMultipleComponent]
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        public event Action OnDialogueComplete;
        private bool isDialogueActive;
        public bool IsDialogueActive => isDialogueActive;

        [SerializeField] private DialogueUI dialogueUI;
        [SerializeField] private GameObject interfaceCanvas;

        private DialogueAsset currentAsset;
        private Dictionary<int, DialogueNode> nodeMap;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (dialogueUI == null)
                dialogueUI = FindObjectOfType<DialogueUI>();
            dialogueUI.Hide();
        }

        public void StartDialogue(DialogueAsset asset)
        {
            Debug.Log($"[DialogueManager] StartDialogue called with asset: {asset?.name}");
            if (asset == null || asset.Nodes == null || asset.Nodes.Count == 0)
                return;
            isDialogueActive = true;
            if (interfaceCanvas != null)
                interfaceCanvas.SetActive(true);
            currentAsset = asset;
            nodeMap = new Dictionary<int, DialogueNode>();
            foreach (var node in asset.Nodes)
                nodeMap[node.Id] = node;
            int startId = asset.Nodes[0].Id;
            ShowNode(startId);
        }

        private void ShowNode(int nodeId)
        {
            if (!nodeMap.TryGetValue(nodeId, out var node))
            {
                EndDialogue();
                return;
            }
            string text = string.IsNullOrEmpty(node.Text) ? "..." : node.Text;
            dialogueUI.Show(node.Speaker, text, node.Options, OnOptionSelected);
        }

        private void OnOptionSelected(DialogueOption option)
        {
            // Выполняем действия
            if (option.Actions != null)
            {
                foreach (var action in option.Actions)
                {
                    switch (action.Type)
                    {
                        case DialogueAction.ActionType.GiveItem:
                            // TODO: реализовать выдачу предмета по параметру
                            break;
                        case DialogueAction.ActionType.TriggerEvent:
                            // TODO: реализовать вызов события по параметру
                            break;
                    }
                }
            }
            if (option.NextNodeId < 0)
                EndDialogue();
            else
                ShowNode(option.NextNodeId);
        }

        private void EndDialogue()
        {
            dialogueUI.Hide();
            if (interfaceCanvas != null)
                interfaceCanvas.SetActive(false);
            isDialogueActive = false;
            currentAsset = null;
            nodeMap = null;
            OnDialogueComplete?.Invoke();
        }
    }
} 