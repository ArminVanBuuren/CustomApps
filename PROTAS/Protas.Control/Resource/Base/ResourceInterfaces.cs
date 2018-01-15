using System;
using Protas.Components.XPackage;

namespace Protas.Control.Resource.Base
{
    /// <summary>
    /// Интерфейс контекста
    /// </summary>
    public interface IResourceContext : IDisposable
    {
        ResourceEntityCollection EntityCollection { get; }
        /// <summary>
        /// Возвращает значение статуса инициализации конектса. Возможно при инициализации конструктора возник какой либо эксепшн
        /// Или на входе был неверный собранный конструктор с неверными значениями.
        /// </summary>
        bool IsIntialized { get; }
        /// <summary>
        /// Метод для создания unusual ресурсов, где контекст создает ресурс основываясь на конструкторе и в том числе основываясь на внутренних преобразованиях
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="constructor"></param>
        /// <returns></returns>
        IResource GetResource(Type resource, ResourceConstructor constructor);
        /// <summary>
        /// Проинициализировать новый хэндлер
        /// </summary>
        /// <param name="pack">конфигурация хэндлера</param>
        /// <returns></returns>
        IHandler GetHandler(XPack pack);
    }

    /// <summary>
    /// Интерфейс непосредственного ресурса
    /// </summary>
    public interface IResource : IDisposable
    {
        /// <summary>
        /// Если текущий компонент уже изменислся или возможно что изменился (явно это можно узнать только при вызове RunComponent)
        /// После инициировании данного эвента, класс ResourceParcer вызывает метод RunComponent чтобы получить явный результат
        /// </summary>
        RHandlerEvent ResourceChanged { get; set; }
        /// <summary>
        /// Конструктор ресурса. Необходимое колличество параметров, необходимые для выполнения ресурса.
        /// </summary>
        ResourceConstructor Constructor { get; }

        /// <summary>
        /// Если ресурс является эвентом, то при вызове GetResult() значения должны отличаться от предыдущих, если тип = InfinityUnique, Specific
        /// Статичный ресурс не может являться эвентом
        /// </summary>
        bool IsTrigger { get; set; }

        /// <summary>
        /// Тип ресурса
        /// </summary>
        ResultType Type { get; }

        /// <summary>
        /// Возвращает значение статуса инициализации реурс. Возможно при инициализации конструктора возник какой либо эксепшн
        /// Или на входе был неверный собранный конструктор с неверными значениями.
        /// </summary>
        bool IsIntialized { get; }
        /// <summary>
        /// Результат ресурса
        /// </summary>
        /// <returns></returns>
        XPack GetResult();

    }

    /// <summary>
    /// Интерфейс непосредственного хэндлера
    /// </summary>
    public interface IHandler
    {
        /// <summary>
        /// Запускаем хэндлер, с входными динамическами параметрами
        /// </summary>
        XPack Run(ResourceComplex complex);
        XPack ForceStop();
    }


}
