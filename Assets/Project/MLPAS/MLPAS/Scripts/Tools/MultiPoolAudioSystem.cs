using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlmenaraGames
{
    public class MultiPoolAudioSystem
    {

        public static bool isNULL = true;
        static MultiAudioManager m_audioManager;
        public static MultiAudioManager audioManager
        {
            get
            {
                if (isNULL)
                {
                    m_audioManager = MultiAudioManager.Instance; isNULL = false;
                }
                return m_audioManager;
            }
            set { m_audioManager = value; }
        }

#if UNITY_2019_2_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            #if UNITY_EDITOR
            isNULL = true;
            #endif
        }
#endif

        /*  public static void ResetStaticVars()
          {
              isNULL = true;
              m_audioManager = null;
          }*/

    }
}