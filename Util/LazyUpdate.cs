using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a value that gets update once per <see cref="updatePeriod"/> frames
/// </summary>
/// <typeparam name="T"></typeparam>
public class LazyUpdate<T> {
    private T _value;
    public T value {
        get {
            if(needUpdate) {
                UpdateValue();
            }
            return _value;
        }
    }
    
    public static implicit operator T(LazyUpdate<T> lazyUpdate) {
        return lazyUpdate.value;
    }

    private int updatePeriod;
    private Func<T> updateFactory;

    private int lastFrameCount = -1;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="updateFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
    public LazyUpdate(Func<T> updateFactory) : this(1, updateFactory) {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="updatePeriod">How many frames have to pass after the last update to recalculate the value.</param>
    /// <param name="updateFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
    public LazyUpdate(int updatePeriod, Func<T> updateFactory) {
        if(updatePeriod <= 0) {
            throw new ArgumentException($"Param {nameof(updatePeriod)} must be positive");
        }
        if(updateFactory == null) {
            throw new ArgumentNullException(nameof(updateFactory));
        }
        this.updatePeriod = updatePeriod;
        this.updateFactory = updateFactory;
    }

    private bool needUpdate {
        get {
            return lastFrameCount < 0 || Time.frameCount - lastFrameCount >= updatePeriod;
        }
    }

    private void UpdateValue() {
        _value = updateFactory();
        lastFrameCount = Time.frameCount;
    }
}
