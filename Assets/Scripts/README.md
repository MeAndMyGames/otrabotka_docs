# Otrabotka - Unity Game Project

## 🎮 Описание
"Otrabotka" - это 2D игра в 3D пространстве, сатира на современную российскую глубинку. Игрок управляет водителем скорой помощи в течение 10 рабочих дней, пытаясь выполнить противоречивые задания в условиях системной коррупции и бюрократии.

## 🏗️ Архитектура

### Core (Ядро системы)
- **`ManagerBase`** - базовый класс для всех менеджеров с жизненным циклом
- **`GameBootstrap`** - центральный инициализатор всех компонентов
- **`ServiceLocator`** - простой контейнер зависимостей

### Systems (Основные системы)
- **`EnvironmentManager`** - управление циклом дня и освещением
- **`MissionTimer`** - таймер миссии с реальным временем
- **`ChunkManager`** - управление инстансами чанков
- **`ChunkSpawner`** - процедурная генерация уровня
- **`TransportController`** - управление посадкой/высадкой из транспорта и сохранение оставленных машин
- **`CameraController`** - управление камерой с поддержкой настроек из `InteractionDefinition` при нахождении в транспорте

### Scenario (Сценарии)
- **`ScenarioDirector`** - главный контроллер сценария
- **`ScenarioSettings`** - хранит параметры генерации чанков и событий дня (HospitalChunks, SimpleChunkConfigs, ComplexEventConfigs, FinalBiomes, TotalChunks)
- **`ChunkSequenceBuilder`** - собирает и распределяет чанки дня на основе `ScenarioSettings` и времени прохождения
- **`ScenarioProgressController`** - отслеживание прогресса событий
- **`EventSequenceBuilder`** - построитель последовательностей событий

### Level (Уровни)
- **`ChunkInstance`** - отдельный чанк уровня
- **`ChunkConfig`** - конфигурация чанков
- **`ChunkTemplateBuffer`** - буфер шаблонов

## 🚀 Быстрый старт

1. **Настройка сцены:**
   - Добавьте `GameBootstrap` на пустой GameObject
   - Настройте `ChunkSpawner` с ссылками на конфиги
   - Добавьте `EnvironmentManager` для освещения

2. **Конфигурация:**
   - Создайте `ScenarioSettings` и заполните поля:
     - Hospital Chunks (список префабов базового биома)
     - HospitalChunkCount (число чанков больницы)
     - Simple Chunk Configs (список простых рандомных чанков)
     - SimpleEventProbability (шанс мелких событий)
     - Complex Event Configs (список `ComplexEventConfig`)
     - Final Biomes (список `FinalBiomeConfig`)
     - TotalChunks (общее число чанков в уровне)
   - Создайте `DayCycleSettings` для управления циклом дня
   - Настройте `ChunkConfig` для каждого типа чанка
   - Заполните `StaticSequenceConfig` событиями

3. **Запуск:**
   - Игра автоматически инициализируется через `GameBootstrap`
   - Чанки начнут генерироваться и двигаться
   - События будут запускаться по расписанию

## 🔧 Основные механики

### Система событий
```csharp
// Создание события
var eventData = new Event
{
    Id = 1001,
    Duration = 30f,
    TimeShiftOnFail = 2f,
    HasSecondaryState = true
};

// Обработка результата
scenarioProgressController.CompleteCurrentEvent(success: false, timeShift: 2f);
```

### Процедурная генерация
```csharp
// Генерация шаблона дня на основе настроек сценария и времени прохождения
chunkTemplateBuffer.GenerateTemplate(scenarioSettings, timeTaken);

// Спавн чанка
chunkSpawner.SpawnAt(index);
```

### Временная система
```csharp
// Сдвиг времени
environmentManager.ShiftTime(2f); // +2 часа

// Проверка таймера
if (missionTimer.RemainingHours <= 0)
{
    // Миссия провалена
}
```

### Система транспорта и взаимодействия
- **`InteractableComponent`** - компонент для входа/выхода из транспорта (нажатие E)
- **`TransportController`** - централизованная логика посадки/высадки, сохранение `ExitInfo` и восстановление машин в чанках
- Поля `InteractionDefinition` для транспорта:
  - `exitEventPrefab` - префаб события высадки
  - `chunkScrollSpeedPositive`, `chunkScrollSpeedNegative` - скорости прокрутки чанков в транспорте
  - `chunkScrollInertiaEnabled`, `chunkScrollInertiaTime` - инерция прокрутки чанков
  - `cameraSettings` - настройки камеры при нахождении в транспорте

## 📁 Структура проекта

```
Assets/Scripts/
├── Core/                   # Базовые классы, ServiceLocator, ManagerBase, GameBootstrap
├── Systems/                # Основные игровые системы (EnvironmentManager, MissionTimer, ChunkManager и т.д.)
├── Level/                  # Система чанков и генерации уровней (ChunkConfig, ChunkInstance, ChunkSpawner)
├── Scenario/               # Сценарии и события (ScenarioDirector, ScenarioProgressController, EventSequenceBuilder)
├── Managers/               # Высокоуровневые контроллеры (ScenarioDirector и т.д.)
├── Interfaces/             # Интерфейсы для взаимосвязей (ICinematicPlayer, IMissionTimer и т.д.)
├── Configs/                # ScriptableObject-конфиги (DayCycleSettings, MissionSettings, ComplexEventConfig)
├── UI/                     # Компоненты пользовательского интерфейса
│   ├── DialogueUI.cs       # Вывод диалогов
│   ├── EventDialogUI.cs    # UI для игровых событий
│   └── Prefabs/            # Префабы кнопок и панелей
├── Shaders/                # Пользовательские шейдеры (LayerPixelize и др.)
└── Framework/              # Дополнительные инструменты и вспомогательный код
```

## 🚀 Быстрый старт
1. Откройте проект в Unity (версия 2020.3+).
2. В сцене `MainScene_v1_test_1` убедитесь, что на GameObject висит `GameBootstrap`.
3. Настройте в инспекторе все ссылки:
   - **ScenarioSettings**, **DayCycleSettings**, **MissionSettings** и другие конфиги из папки `Configs/`.
   - **DialogueManager**: ссылка на `DialogueUI` и `Interface_Canvas`.
   - **DialogueUI**: ссылка на `DialoguePanel`, `SpeakerText`, `ContentText`, `OptionsContainer` и префаб `OptionButton`.
4. Запустите сцену — игра инициализируется автоматически.

## 🏗️ Основные составляющие

### Система диалогов
- **`DialogueAsset`**, **`DialogueNode`**, **`DialogueOption`** — структуры данных для описания ветвлений.
- **`DialogueManager`** (Singleton) — управляет показом ветвлений, хранит словарь узлов, вызывает события `OnDialogueComplete`.
- **`DialogueUI`** — отображает текст, кнопки ответов и «Закончить диалог», синхронизирует активацию панели `DialoguePanel`.
- **`DialogueTrigger`** — запускает диалог при касании игроком триггеров и восстанавливает `Time.timeScale` после завершения.

### Система событий
- **`EventDialogUI`** — UI окон выбора (успех/неудача) для ключевых игровых событий.
- **`ScenarioProgressController`** — отслеживает прогресс событий, вызывает вызовы в `ScenarioDirector`.

### Процедурная генерация уровней
- **`ChunkSequenceBuilder`** и **`EventSequenceBuilder`** — собирают шаблон дня и последовательность событий.
- **`ChunkSpawner`** и **`ChunkManager`** — инстанцируют и управляют движением чанков.

### Временная система
- **`DaysCycleSettings`** и **`EnvironmentManager`** — контролируют освещение и смену дня/ночи.
- **`MissionTimer`** — отслеживает оставшееся время миссии.

### Шейдеры
- **`LayerPixelize.shader`** — пикселизация экрана с добавлением «шумовых хлопьев», включает параметр цветового тинта (`_Color`) и Pass для отбрасывания теней.

## 🎯 Текущий статус и планы
1. Реализована базовая система диалогов и система событий.
2. Настроена процедурная генерация уровней и базовый UI.
3. Добавлен шейдер `LayerPixelize` с возможностью настройки цвета и теней.

**Планы на будущее:**
- Расширить контент диалогов и событий.
- Внедрить систему коррупции и экономику.
- Улучшить UI/UX, добавить анимации и звуки.
- Оптимизировать производительность и заполнить тестами ключевую логику.

## 🤝 Вклад в проект
Проект использует:
- Паттерн **Service Locator** для управления зависимостями.
- **Manager Pattern** для организации систем.
- **Event-driven architecture** для коммуникации между компонентами.
- **Object Pooling** для оптимизации работы с чанками.

## 📝 Лицензия
Этот проект является сатирическим произведением и распространяется свободно для образовательных и исследовательских целей. 