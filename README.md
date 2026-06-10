

## Основные компоненты

### 1. Ядро (Core)
| Компонент | Ответственность |
|-----------|-----------------|
| `ISceneObject` | Интерфейс для всех объектов сцены |
| `SceneObject` | Реализация, MonoBehaviour-обёртка |

### 2. Менеджеры (Managers)
| Компонент | Ответственность | Паттерн |
|-----------|-----------------|---------|
| `GameSceneManager` | Генерация, регистрация, удаление объектов | Синглтон + Фасад |
| `ObjectSelector` | Выбор объекта (клик/программно), фокус камеры | Синглтон |
| `UIController` | Создание/удаление UI элементов, скролл | Синглтон |

### 3. Управление (Controllers)
| Компонент | Ответственность |
|-----------|-----------------|
| `CameraController` | Orbit-камера: вращение (ЛКМ), зум (скролл), панорамирование (ПКМ) |
| `ColorPickerIntegration` | Синхронизация ColorPicker с цветом выбранного объекта |

### 4. UI (Presentation)
| Компонент | Ответственность |
|-----------|-----------------|
| `UIElement` | Отдельный элемент списка (кнопки Delete/Select, имя, подсветка) |
| `SliderListener` | Слайдер выбора количества объектов в начале |


## Потоки данных

Когда пользователь нажимает кнопку AddObject, GameSceneManager генерирует новый объект в случайном месте перед камерой, регистрирует его в своём списке и отправляет событие OnObjectAdded. На это событие подписан UIController, который создаёт новый UIElement для этого объекта, и ObjectSelector, который может автоматически выбрать новый объект.

Когда пользователь нажимает кнопку Delete на UIElement, этот элемент отправляет событие RequestDeleted в UIController. UIController вызывает у GameSceneManager метод RemoveObject, тот удаляет объект из списка, отправляет событие OnObjectRemoved и уничтожает GameObject. ObjectSelector подписан на это событие и при удалении выбранного объекта автоматически выбирает другой случайный объект.

Выбор объекта может произойти двумя способами. Первый: пользователь кликает по 3D-объекту на сцене, ObjectSelector делает Raycast и вызывает SelectObject. Второй: пользователь кликает на UIElement, тот вызывает событие OnSelected, которое ловит UIController и вызывает SelectObject у ObjectSelector.

В любом случае после выбора ObjectSelector отправляет событие OnSelectionChanged. На него подписаны UIController для подсветки выбранного элемента и скролла к нему, ColorPickerIntegration для обновления цвета в пикере и CameraController для фокусировки камеры на выбранном объекте.




## Событийная модель

| Событие | Источник | Подписчики |
|---------|----------|------------|
| `OnObjectAdded` | GameSceneManager | UIController, ObjectSelector |
| `OnObjectRemoved` | GameSceneManager | ObjectSelector |
| `OnSelectionChanged` | ObjectSelector | UIController, ColorPickerIntegration |
| `RequestDeleted` | UIElement | UIController |
| `OnSelected` | UIElement | UIController |
| `OnDestroying` | SceneObject | (никто, задел на будущее) |



## Известные траблы

1. **Жесткая связь через синглтоны** — отсутствует DI, сложно тестировать
2. **GameSceneManager нарушает SRP** — генерация + хранение + регистрация
3. **Смешение ответственности**

   

## Что сделал бы в продакшене

1. Внедрить нормальный DI
2. Разделить GameSceneManager - ObjectPool + Spawner + ObjectRegistry
3. Вынести настройки камеры в ScriptableObject
4. Добавить пулинг для UI элементов
