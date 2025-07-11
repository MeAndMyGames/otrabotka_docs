using System;
using System.Collections.Generic;
using UnityEngine;

namespace Otrabotka.Scenario.Configs
{
    [Serializable]
    public class DialogueNode
    {
        [Tooltip("Уникальный идентификатор узла")]
        public int Id;

        [Tooltip("Говорящий (имя персонажа)")]
        public string Speaker;

        [Tooltip("Текст реплики; если пуст, отображается '...'")]
        [TextArea]
        public string Text;

        [Tooltip("Список доступных вариантов ответа")]
        public List<DialogueOption> Options = new List<DialogueOption>();
    }

    [Serializable]
    public class DialogueOption
    {
        [Tooltip("Текст варианта ответа игрока или кнопки")]
        public string Text;

        [Tooltip("Идентификатор следующего узла диалога; -1 для завершения")]        
        public int NextNodeId = -1;

        [Tooltip("Действия, выполняемые при выборе этого варианта")]        
        public List<DialogueAction> Actions = new List<DialogueAction>();
    }

    [Serializable]
    public class DialogueAction
    {
        public enum ActionType
        {
            None,
            GiveItem,
            TriggerEvent
        }

        [Tooltip("Тип действия")]        
        public ActionType Type;

        [Tooltip("Параметр действия (например, ID предмета или события)")]
        public string Parameter;
    }
} 