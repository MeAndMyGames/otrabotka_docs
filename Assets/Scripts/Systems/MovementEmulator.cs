using UnityEngine;

namespace Otrabotka.Systems
{
    /// <summary>
    /// Эмулирует движение персонажа/камеры для тестирования спавна чанков.
    /// Перемещает привязанный объект по оси X на основе ввода пользователя.
    /// </summary>
    public class MovementEmulator : MonoBehaviour
    {
        [Tooltip("Скорость перемещения (юниты в секунду)")]
        [SerializeField] private float speed = 5f;

        void Update()
        {
            // Горизонтальный ввод: стрелки или A/D
            float h = Input.GetAxis("Horizontal");
            if (Mathf.Abs(h) > 0.01f)
            {
                transform.Translate(h * speed * Time.deltaTime, 0f, 0f, Space.World);
            }
        }
    }
} 