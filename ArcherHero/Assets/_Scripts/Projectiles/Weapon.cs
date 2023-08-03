using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using UnityEngine;
using System.Collections.Generic;

public class Weapon
{
    private const string _pathDefaultProjectiles = "DefaultProjectiles";

    private readonly List<Projectile> _projectiles;

    private readonly Dictionary<TypeDamage, Projectile> _projectilesDictionary;

    private CancellationTokenSource _cancellationTokenSource;

    private IProjectileMovement _currentMovement;
    private TypeDamage _currentTypeDamage;

    public Weapon(ProjectilePrefabsScriptable projectilePrefabsScriptable = null)
    {
        if (projectilePrefabsScriptable != null)
        {
            _projectiles = projectilePrefabsScriptable.ProjectilesPrefabs;
        }
        else 
        {
            _projectiles = Resources.Load<ProjectilePrefabsScriptable>(_pathDefaultProjectiles).ProjectilesPrefabs;
        }

        _currentMovement = new DefaultProjectileMovement();

        _projectilesDictionary = new Dictionary<TypeDamage, Projectile>();

        CompletingDictionary();
    }

    public void StartAttack(Transform target, Transform pointSpawnProjectile, TypeDamage typeDamage, int damage, int attackSpeedPerMinute)
    {
        _currentTypeDamage = typeDamage;
        _cancellationTokenSource = new CancellationTokenSource();

        if (!_projectilesDictionary.ContainsKey(typeDamage))
        {
            throw new Exception("TypeDamage is not in the _projectileDictionary");
        }

        _ = InstantiateSpellAsync(attackSpeedPerMinute, damage, pointSpawnProjectile, target, _cancellationTokenSource.Token);
    }
    public void StopAttack()
    {
        _cancellationTokenSource.Cancel();
    }

    private async UniTaskVoid InstantiateSpellAsync(int attackSpeedPerMinute, int damage, Transform pointSpawnSpell, Transform target, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            Projectile newSpell = GameObject.Instantiate(_projectilesDictionary[_currentTypeDamage], pointSpawnSpell.position, pointSpawnSpell.rotation, null);
            newSpell.Initialize(damage, _currentMovement);
            newSpell.transform.LookAt(target);

            await UniTask.Delay(TimeSpan.FromMinutes(ShotDelay(attackSpeedPerMinute)), cancellationToken: token).SuppressCancellationThrow();
        }
    }

    private void CompletingDictionary()
    {
        foreach (var projectile in _projectiles)
        {
            if (_projectilesDictionary.ContainsKey(projectile.TypeDamage))
            {
                throw new Exception("TypeDamage is already in the _projectilesDictionary");
            }

            _projectilesDictionary.Add(projectile.TypeDamage, projectile);
        }
    }

    private double ShotDelay(float attackSpeedPerMinute)
    {
        return 1 / attackSpeedPerMinute;
    }
}