﻿using System.Collections.Generic;

public class ThreeCreatedProjectile : CreateProjectilesAround
{
    private readonly List<float> _anglesRotation = new()
    {
        0,
        15,
        -15,
    };

    protected override List<float> AnglesRotation => _anglesRotation;
}
