using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Таймер, предоставляющий удобный способ производить некоторые действия раз
/// в некоторый промежуток времени. В отличие от <see cref="UnscaledTimer"/>,
/// зависит от значения <see cref="Time.timeScale"/>
/// </summary>
public class Timer {
    private float origin;
    private float _interval;
    private bool isOnEdge = false;

    protected virtual float currentTime => Time.time;

    public float interval {
        get { return _interval; }
        set {
            if(value < 0)
                Debug.LogWarning($"Negative interval was set: {value}");
            _interval = value;
        }
    }
    
    public float remaining {
        get => Mathf.Max(0, origin + interval - currentTime);
        set {
            if(value < 0) {
                throw new System.ArgumentOutOfRangeException(nameof(value), $"Negative remaining time was set: {value}");
            }
            origin = value + currentTime - interval;
        }
    }

    /// <summary>
    /// Создаёт периодический таймер с интервалом <paramref name="interval"/>.
    /// ВНИМАНИЕ: не используйте конструктор в инициализации полей MonoBehaviour. Это приводит к ошибке.
    /// Вместо этого вызывайте конструктор в Awake или Start.
    /// </summary>
    /// <param name="interval">Интервал таймера.</param>
    public Timer(float interval) {
        this.interval = interval;
        this.origin = currentTime;
    }

    /// <summary>
    /// Устанавливает таймер в такое состояние, что при следующем вызове метода <see cref="Tick"/> таймер гарантированно сработает.
    /// </summary>
    public void SetOnEdge() {
        isOnEdge = true;
    }

    /// <summary>
    /// Отменяет действие вызыванного ранее <see cref="SetOnEdge"/>.
    /// </summary>
    private void RevertSetOnEdge() {
        isOnEdge = false;
    }

    /// <summary>
    /// Сбрасывает таймер. Также отменяет действие вызыванного ранее <see cref="SetOnEdge"/>.
    /// </summary>
    public void Reset() {
        isOnEdge = false;
        origin = currentTime;
    }

    /// <summary>
    /// Обновляет состояние таймера. Если время вышло, заводит его заново.
    /// <returns>Возвращает <see langword="true"/>, если время вышло, иначе <see langword="false"/>.</returns>
    /// </summary>
    public bool Tick() {
        if(isOnEdge || remaining == 0) {
            Reset();
            return true;
        }
        return false;
    }

    public override string ToString() {
        const int progressbarLength = 8;
        float t = 1 - remaining / interval;
        string progressbar;
        if(!float.IsNaN(t)) {
            int progress = Mathf.RoundToInt(progressbarLength * t);
            progressbar = new string('■', progress) + new string('□', progressbarLength - progress);            
        }
        else {
            progressbar = new string('-', progressbarLength);
        }
        return $"[{progressbar}] {currentTime - origin:0.000}/{interval:0.000}";
    }
}

/// <summary>
/// Таймер, не зависящий от значения <see cref="Time.timeScale"/>.
/// </summary>
public class UnscaledTimer : Timer {
    protected override float currentTime => Time.unscaledTime;

    /// <summary>
    /// Создаёт периодический независимый от <see cref="Time.timeScale"/> таймер с интервалом <paramref name="interval"/>.
    /// </summary>
    /// <param name="interval">Интервал таймера.</param>
    public UnscaledTimer(float interval) : base(interval) {
    }
}