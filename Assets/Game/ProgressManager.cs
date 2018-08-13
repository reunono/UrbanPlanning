using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.MMInterface;
using System;
using Cinemachine;

namespace MoreMountains.LDJAM42
{
    public class ProgressManager : PersistentSingleton<ProgressManager>
    {
        public int CurrentLevel = 1;
    }    
}
