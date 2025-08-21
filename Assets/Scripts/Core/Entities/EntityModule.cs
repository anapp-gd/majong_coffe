using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public static class EntityModule
{
    private static Dictionary<Type, SourceEntity> _dictionaryEntities = new Dictionary<Type, SourceEntity>();

    public static void Initialize()
    {
        Debug.Log("Start finding all entities to register");

        var datableTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => typeof(SourceEntity).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        foreach (var type in datableTypes)
        {
            // Create instances and add to save
            try
            {
                if (Activator.CreateInstance(type) is SourceEntity datableInstance)
                {
                    _dictionaryEntities.Add(type, datableInstance);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        var entities = _dictionaryEntities.Values.ToList();

        foreach (var entity in entities)
        {
            entity.Init();
        }

        Debug.Log("All entities registered successfully");
    }

    public static T GetEntity<T>() where T : SourceEntity
    {
        if (_dictionaryEntities.TryGetValue(typeof(T), out var entity))
        {
            return (T)entity;
        }

        throw new KeyNotFoundException($"Entity of type {typeof(T)} not found in dictionary");
    }
}