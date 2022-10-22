using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlmenaraGames;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

namespace AlmenaraGames
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Almenara Games/MLPAS/Animator SFX Controller",10)]
    [HelpURL("https://almenaragames.github.io/#CSharpClass:AlmenaraGames.MLPASAnimatorSFXController")]
    public class MLPASAnimatorSFXController : MonoBehaviour
    {

        public enum ValueToOverride
        {
            UseDifferentPlayPosition,
            PlayPosition,
            FollowPosition
        }

        public enum StateValueToOverride
        {
            PlayTime,
            AudioObject,
            Channel,
            IgnoreListenerPause,
            CustomMethodParameters
        }

        [System.Serializable]
        public class ValuesOverride
        {

            public bool HASID;

            public int ID;

            public string stateName;

            public int layer;

            public bool useDifferentPlayPosition;

            public bool overrideStateSFXsValues;

            public Transform playPosition;

            public bool followPosition;

            [System.Serializable]
            public class StateValue
            {
                public int ID;
                public MLPASAnimatorSFX.StateSFX.OverrideValues Values;

            }

            public bool HasStateValue(int _id)
            {
                return statesValues.Exists(x => x.ID == _id);
            }

            public MLPASAnimatorSFX.StateSFX.OverrideValues GetStateValue(int _id)
            {
                return statesValues.Find(x => x.ID == _id).Values;
            }

            public int GetStateValueIndex(int _id)
            {
                return statesValues.FindIndex(x => x.ID == _id);
            }

            public void AddStateValue(int _id, MLPASAnimatorSFX.StateSFX.OverrideValues _value)
            {
                if (HasStateValue(_id))
                {
                    statesValues.Find(x => x.ID == _id).Values = _value;
                }
                else
                {
                    StateValue _newValue = new StateValue();
                    _newValue.Values = _value;
                    _newValue.ID = _id;
                    statesValues.Add(_newValue);
                }
            }

            public void RemoveStateValue(int _id)
            {
                statesValues.RemoveAt(statesValues.FindIndex(x => x.ID == _id));
            }

            public List<StateValue> statesValues = new List<StateValue>();

        }

        [System.Serializable]
        public class InspectorDelegate
        {

            public Component target;
            public string methodName;
            public bool removed = false;

        }

        public List<InspectorDelegate> inspectorDelegates = new List<InspectorDelegate>();

        public delegate void CustomPlayMethod(MLPASACustomPlayMethodParameters parameters);

        public List<ValuesOverride> newValues = new List<ValuesOverride>();
        Dictionary<string, CustomPlayMethod> customPlayMethods = new Dictionary<string, CustomPlayMethod>();

        List<MLPASAnimatorSFX.StateSFX> states = new List<MLPASAnimatorSFX.StateSFX>();

        List<CustomPlayMethod> registeredPlayMethods=new List<CustomPlayMethod>();

        Animator anim;

        bool inspectorDelegatesAdded = false;


        /// <summary>
        /// Register a Custom Method to be used by the Animator next to this <see cref="MLPASAnimatorSFXController"/>.
        /// </summary>
        /// <param name="method"></param>
        public void RegisterCustomMethod(CustomPlayMethod method)
        {

            if (method == null)
                return;

            string methodName = method.Method.Name;

            if (customPlayMethods.ContainsKey(methodName))
            {
                Debug.LogWarning("<i>'" + methodName + "'</i>" + " Can't be registered, a Custom Play Method with the same name is already registered.");
            }
            else
            {
                customPlayMethods.Add(methodName, method);
            }

            for (int i = 0; i < states.Count; i++)
            {
                MLPASAnimatorSFX.StateSFX state = states[i];

                if (state.useCustomPlayMethod && state.methodName == methodName)
                    states[i].customPlayMethod = method;
            }

            if (!registeredPlayMethods.Contains(method))
                registeredPlayMethods.Add(method);

        }

        void RegisterCustomMethod(CustomPlayMethod method, bool showWarning)
        {

            if (method == null)
                return;

            string methodName = method.Method.Name;

            if (customPlayMethods.ContainsKey(methodName))
            {
                if (showWarning)
                {
                    Debug.LogWarning("<i>'" + methodName + "'</i>" + " Can't be registered, a Custom Play Method with the same name is already registered.");
                }
            }
            else
            {
                customPlayMethods.Add(methodName, method);
            }

            for (int i = 0; i < states.Count; i++)
            {
                MLPASAnimatorSFX.StateSFX state = states[i];

                if (state.useCustomPlayMethod && state.methodName == methodName)
                {
                    states[i].customPlayMethod = method;


                }
            }

        }

        /// <summary>
        /// Unregister a Custom Play Method in this <see cref="MLPASAnimatorSFXController"/>.
        /// </summary>
        /// <param name="method"></param>
        public void UnregisterCustomMethod(CustomPlayMethod method)
        {
            if (method == null)
                return;

            string methodName = method.Method.Name;

            if (customPlayMethods.ContainsKey(methodName))
            {
                customPlayMethods.Remove(methodName);

                for (int i = 0; i < states.Count; i++)
                {
                    MLPASAnimatorSFX.StateSFX state = states[i];

                    if (state.useCustomPlayMethod && state.methodName == methodName)
                        states[i].customPlayMethod = null;
                }
            }
            else
            {
                Debug.LogWarning("Custom Play Method: " + "<i>'" + methodName + "'</i>" + " is already unregisted.");
            }

            if (registeredPlayMethods.Contains(method))
                registeredPlayMethods.Remove(method);

        }

        /// <summary>
        /// Change a Value of The Custom Position For The Specific AnimatorSFX in the Animator next to this <see cref="MLPASAnimatorSFXController"/>.
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="layer"></param>
        /// <param name="valueToOverride"></param>
        /// <param name="value">UseDifferentPlayPosition: needs to be a boolean value | PlayPosition: needs to be a Transform value | FollowPosition: needs to be a boolean value</param>.
        public void SetStateBehaviourCustomPosition(int stateBehaviourID, ValueToOverride valueToOverride, object value)
        {

            bool iDValidated = false;

            if (newValues.Exists(x => x.HASID && x.ID == stateBehaviourID))
                iDValidated = true;

            if (!iDValidated)
                return;

            switch (valueToOverride)
            {
                case ValueToOverride.UseDifferentPlayPosition:

                    if (value is bool)
                        newValues.Find(x => x.HASID && x.ID == stateBehaviourID).useDifferentPlayPosition = (bool)value;
                    else
                        Debug.LogError("<b>UseDifferentPlayPosition</b> needs to be a boolean value");

                    break;
                case ValueToOverride.PlayPosition:

                    if (value is Transform)
                        newValues.Find(x => x.HASID && x.ID == stateBehaviourID).playPosition = (Transform)value;
                    else
                        Debug.LogError("<b>PlayPosition</b> needs to be a Transform value");

                    break;
                case ValueToOverride.FollowPosition:

                    if (value is bool)
                        newValues.Find(x => x.HASID && x.ID == stateBehaviourID).followPosition = (bool)value;
                    else
                        Debug.LogError("<b>FollowPosition</b> needs to be a boolean value");

                    break;
            }



        }

        /// <summary>
        /// Change a Value of a specific StateSFX in the Animator next to this <see cref="MLPASAnimatorSFXController"/>.
        /// </summary>
        /// <param name="stateBehaviourID"></param>
        /// <param name="stateSFXIndex"></param>
        /// <param name="valueToOverride"></param>
        /// <param name="value">PlayTime: needs to be a float value | AudioObject: needs to be an AudioObject or string value | Channel: needs to be a int value  | IgnoreListenerPause: needs to be a bool value | CustomMethodParameters: needs to be a int, boolean, float, string or object value</param>
        public void OverrideStateSFXValue(int stateBehaviourID, int stateSFXIndex, StateValueToOverride valueToOverride, object value)
        {

            bool iDValidated = false;

            if (newValues.Exists(x => x.HASID && x.ID == stateBehaviourID))
                iDValidated = true;

            if (states.Count < stateSFXIndex)
                iDValidated = false;

            if (!iDValidated)
                return;

            int _stateID = states[stateSFXIndex].ID;


            switch (valueToOverride)
            {
                case StateValueToOverride.PlayTime:

                    if (value is float)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverridePlayTime = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.playTime = (float)value;
                    }
                    else
                        Debug.LogError("<b>PlayTime</b> needs to be a float value");

                    break;
                case StateValueToOverride.AudioObject:

                    if (value is AudioObject)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideAudioObject = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.useIdentifier = false;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.audioObject = (AudioObject)value;
                    }
                    else if (value is string)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideAudioObject = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.useIdentifier = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.identifier = (string)value;
                    }
                    else
                        Debug.LogError("<b>AudioObject</b> needs to be an AudioObject or string value");

                    break;
                case StateValueToOverride.Channel:

                    if (value is int)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideChannel = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.useChannel = (int)value >= 0 ? true : false;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.channel = (int)value;
                    }
                    if (value is bool)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideChannel = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.useChannel = (bool)value;
                    }
                    else
                        Debug.LogError("<b>Channel</b> needs to be an int or boolean value");

                    break;
                case StateValueToOverride.IgnoreListenerPause:

                    if (value is bool)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideListenerPause = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.ignoreListenerPause = (bool)value;
                    }
                    else
                        Debug.LogError("<b>IgnoreListenerPause</b> needs to be a boolean value");

                    break;
                case StateValueToOverride.CustomMethodParameters:

                    if (value is MLPASACustomPlayMethodParameters.CustomParams)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters = true;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams = (MLPASACustomPlayMethodParameters.CustomParams)value;
                    }
                    else if (value is int)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        bool _alreadyOverride = _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters = true;
                        MLPASACustomPlayMethodParameters.CustomParams _params = states[stateSFXIndex].userParameters;
                        if (_alreadyOverride)
                        {
                            _params = _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams;
                        }
                        _params.IntParameter = (int)value;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams = _params;
                    }
                    else if (value is string)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        bool _alreadyOverride = _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters = true;
                        MLPASACustomPlayMethodParameters.CustomParams _params = states[stateSFXIndex].userParameters;
                        if (_alreadyOverride)
                        {
                            _params = _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams;
                        }
                        _params.StringParameter = (string)value;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams = _params;
                    }
                    else if (value is float)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        bool _alreadyOverride = _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters = true;
                        MLPASACustomPlayMethodParameters.CustomParams _params = states[stateSFXIndex].userParameters;
                        if (_alreadyOverride)
                        {
                            _params = _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams;
                        }
                        _params.FloatParameter = (float)value;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams = _params;
                    }
                    else if (value is bool)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        bool _alreadyOverride = _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters = true;
                        MLPASACustomPlayMethodParameters.CustomParams _params = states[stateSFXIndex].userParameters;
                        if (_alreadyOverride)
                        {
                            _params = _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams;
                        }
                        _params.BoolParameter = (bool)value;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams = _params;
                    }
                    else if (value is Object)
                    {
                        ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == stateBehaviourID);
                        _value.overrideStateSFXsValues = true;
                        bool _alreadyOverride = _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.OverrideCustomParameters = true;
                        MLPASACustomPlayMethodParameters.CustomParams _params = states[stateSFXIndex].userParameters;
                        if (_alreadyOverride)
                        {
                            _params = _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams;
                        }
                        _params.ObjectParameter = (Object)value;
                        _value.statesValues[_value.GetStateValueIndex(_stateID)].Values.customParams = _params;
                    }
                    else
                    Debug.LogError("<b>CustomMethodParameters</b> needs to be a \"<b>MLPASACustomPlayMethodParameters.CustomParams</b>\" or a int, boolean, float, string or object value");

                    break;

            }



        }

        /// <summary>
        /// Change a Value of The Custom Position For The Specific AnimatorSFX in the Animator next to this <see cref="MLPASAnimatorSFXController"/>.
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="layer"></param>
        /// <param name="valueToOverride"></param>
        /// <param name="value">UseDifferentPlayPosition: needs to be a boolean value | PlayPosition: needs to be a Transform value | FollowPosition: needs to be a boolean value</param>.
        public void SetStateBehaviourCustomPosition(string stateName, int layer, ValueToOverride valueToOverride, object value)
        {

            bool iDValidated = false;

            if (newValues.Exists(x => x.stateName == stateName && x.layer == layer))
                iDValidated = true;

            if (!iDValidated)
                return;

            int _behaviourID = newValues.Find(x => x.stateName == stateName && x.layer == layer).ID;

            SetStateBehaviourCustomPosition(_behaviourID, valueToOverride, value);

        }

        /// <summary>
        /// Change a Value of a specific StateSFX in the Animator next to this <see cref="MLPASAnimatorSFXController"/>.
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="layer"></param>
        /// <param name="stateSFXIndex"></param>
        /// <param name="valueToOverride"></param>
        /// <param name="value">PlayTime: needs to be a float value | AudioObject: needs to be an AudioObject or string value | Channel: needs to be a int value  | IgnoreListenerPause: needs to be a bool value | CustomMethodParameters: needs to be a int, boolean, float, string or object value</param>
        public void OverrideStateSFXValue(string stateName, int layer, int stateSFXIndex, StateValueToOverride valueToOverride, object value)
        {

            bool iDValidated = false;

            if (newValues.Exists(x => x.stateName == stateName && x.layer == layer))
                iDValidated = true;

            if (states.Count <= stateSFXIndex)
                iDValidated = false;

            if (!iDValidated)
                return;

            int _behaviourID = newValues.Find(x => x.stateName == stateName && x.layer == layer).ID;


            OverrideStateSFXValue(_behaviourID, stateSFXIndex, valueToOverride, value);

        }

        bool ContainsState(AlmenaraGames.MLPASAnimatorSFX state, out ValuesOverride valuesOverride)
        {

            if (newValues.Exists(x => x.HASID && x.ID == state.id || x.stateName == state.runtimeStateName && x.layer==state.transitionLayer  ))
            {
                ValuesOverride _value = newValues.Find(x => x.HASID && x.ID == state.id || x.stateName == state.runtimeStateName && x.layer == state.transitionLayer);

                _value.stateName = state.runtimeStateName;
                _value.layer = state.transitionLayer;

                valuesOverride = _value;


                return true;
            }

            valuesOverride = null;
            return false;
        }



        bool previousEnabled = false;

        private void Update()
        {
            
            if (previousEnabled!=anim.enabled)
            {
                if (anim.enabled)
                {
                    UpdateValues();
                }
                previousEnabled = anim.enabled;

            }

 

        }

        void OnDisable()
        {
            previousEnabled = false;
        }

        void OnEnable()
        {
          
            if (previousEnabled != anim.enabled)
            {
                if (anim.enabled)
                {
                    UpdateValues();
                }
                previousEnabled = anim.enabled;
            }
        }

        void Awake()
        {
            anim = GetComponent<Animator>();

            if (anim == null)
            {
                Debug.LogError("Component: <b>MLPASAnimatorSFXControlller</b> needs to be placed next to an <i>Animator Controller</i>");
            }
            else
            {
                UpdateValues();
                previousEnabled = anim.enabled;
            }
        }

        void OnDrawGizmosSelected()
        {

            Gizmos.DrawIcon(transform.position, "AlmenaraGames/MLPAS/AnimatorSFXControllerIco");

        }

        void OnDrawGizmos()
        {

           //HIDE

        }

        public void UpdateValues()
        {

            if (anim != null)
            {
                bool n = false;

                states.Clear();

                foreach (var item in anim.GetBehaviours<AlmenaraGames.MLPASAnimatorSFX>())
                {

                    ValuesOverride newValue = null;

                    item.trf = transform;

                    if (ContainsState(item, out newValue))
                    {

                        newValue.stateName = item.runtimeStateName;
                        newValue.layer = item.transitionLayer;

                        item.AssignSFXController(this,newValue);

                    }

                   // if (!statesAdded)
                    
                        states.AddRange(item.stateSfxs);
                    


                    n = true;
                }

                if (!n)
                {
                    Debug.LogWarning("The Animator from Game Object: <b>" + gameObject.name + "</b> doesn't have any <i>MLPASAnimatorSFX</i> State Machine Behaviour");
                }
                else
                {

                    if (!inspectorDelegatesAdded)
                    {
                        foreach (var item in inspectorDelegates)
                        {

                            if (item.target == null || string.IsNullOrEmpty(item.methodName))
                            {
                                continue;
                            }

                            System.Reflection.MethodInfo[] methods = item.target.GetType().GetMethods();
                            System.Reflection.MethodInfo correctMethod = null;

                            for (int i = 0; i < methods.Length; i++)
                            {

                                bool validMethod = false;
                                System.Reflection.ParameterInfo[] parameters = methods[i].GetParameters();

                                for (int i2 = 0; i2 < parameters.Length; i2++)
                                {

                                    if (item.methodName == methods[i].Name && parameters[i2].ParameterType == typeof(MLPASACustomPlayMethodParameters))
                                    {
                                        correctMethod = methods[i];
                                        validMethod = true;
                                        break;
                                    }


                                }

                                if (validMethod)
                                    break;

                            }

                            CustomPlayMethod action = (CustomPlayMethod)System.Delegate.CreateDelegate(typeof(CustomPlayMethod), item.target, correctMethod);

                            if (correctMethod != null && action != null)
                            {
                                registeredPlayMethods.Add(action);
                                RegisterCustomMethod(action, false);
                            }


                        }
                        inspectorDelegatesAdded = true;
                    }
                    

                    foreach (var m in registeredPlayMethods)
                    {
                        RegisterCustomMethod(m, false);
                    }
                }
            }


        }




#if UNITY_EDITOR
        [CustomEditor(typeof(MLPASAnimatorSFXController))]
        public class MLPASAnimatorSFXControllerEditor : UnityEditor.Editor
        {

            SerializedObject obj;

            Color color_selected = new Color32(98, 220, 255, 255);
            Color colorPro_selected = new Color32(28, 128, 170, 255);

            private Texture2D uiBack;

            bool valuesModified = false;
            bool dirty = false;

            int selectedIndexStateM = 0;
            int prevSelectedIndexStateM = 0;

            bool animatorValidated = false;

            GameObject customPlayMethodObj;
            int playMethodIndex;

            public UnityEditor.Animations.AnimatorController anim;

            void CheckMethods()
            {
                if (!EditorApplication.isPlaying && (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates!=null && (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.Count>0)
                {

                    foreach (var item in (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates)
                    {

                        System.Reflection.MethodInfo[] methods = item.target.GetType().GetMethods();
                        System.Reflection.MethodInfo correctMethod = null;

                        for (int i = 0; i < methods.Length; i++)
                        {

                            bool validMethod = false;
                            System.Reflection.ParameterInfo[] parameters = methods[i].GetParameters();

                            for (int i2 = 0; i2 < parameters.Length; i2++)
                            {

                                if (item.methodName == methods[i].Name && parameters[i2].ParameterType == typeof(MLPASACustomPlayMethodParameters))
                                {
                                    correctMethod = methods[i];
                                    validMethod = true;
                                    break;
                                }


                            }

                            if (validMethod)
                                break;

                        }

                        if (correctMethod == null || item.target == null || string.IsNullOrEmpty(item.methodName))
                        {
                            item.removed = true;
                        }
                        else
                        {
                            item.removed = false;
                        }
                    }

                    SetObjectDirty((obj.targetObject as MLPASAnimatorSFXController));
                }

            }

            void OnDisable()
            {

                if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode && obj!=null && obj.targetObject!=null && (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates != null && (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.Count > 0)
                {
                    for (int i = 0; i < (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.Count; i++)
                    {
                        if ((obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].removed)
                        {
                            (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.RemoveAt(i);

                        }
                    }

                    SetObjectDirty((obj.targetObject as MLPASAnimatorSFXController));
                }

            }

            void OnEnable()
            {
                uiBack = Resources.Load("MLPASImages/guiBack") as Texture2D;
               
                obj = new SerializedObject(target);



                CheckMethods();

            }

            bool ContainsStateEditor(AlmenaraGames.MLPASAnimatorSFX state, out ValuesOverride valuesOverride)
            {
                
                UnityEditor.Animations.StateMachineBehaviourContext[] context = UnityEditor.Animations.AnimatorController.FindStateMachineBehaviourContext(state);
                UnityEditor.Animations.AnimatorState cState = (context[0].animatorObject as UnityEditor.Animations.AnimatorState);
                UnityEditor.Animations.AnimatorStateMachine cStateMachine = (context[0].animatorObject as UnityEditor.Animations.AnimatorStateMachine);

                string stateName = cState != null ? cState.name : cStateMachine.name;
                int layer = context[0].layerIndex;

                if ((obj.targetObject as MLPASAnimatorSFXController).newValues.Exists(x => x.HASID && x.ID==state.id || x.stateName == stateName && x.layer == layer))
                {
                    ValuesOverride _value = (obj.targetObject as MLPASAnimatorSFXController).newValues.Find(x => x.HASID && x.ID == state.id || x.stateName == stateName && x.layer == layer);

                    _value.stateName = stateName;
                    _value.layer = layer;

                    valuesOverride = _value;

                    valuesOverride = (obj.targetObject as MLPASAnimatorSFXController).newValues.Find(x => x.HASID && x.ID == state.id || x.stateName == stateName && x.layer == layer);
                    return true;
                }

                valuesOverride = null;
                return false;
            }

            public override void OnInspectorGUI()
            {
              
                obj.Update();

                animatorValidated = false;

                Animator animComponent = (obj.targetObject as MLPASAnimatorSFXController).GetComponent<Animator>();

                if (animComponent != null)
                {
                    string assetPath = AssetDatabase.GetAssetPath(animComponent.runtimeAnimatorController);

                    UnityEditor.Animations.AnimatorController newAnim = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(assetPath);

                    if (newAnim == null)
                    {
                        AnimatorOverrideController newAnimOverride = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(assetPath);

                        if (newAnimOverride != null)
                            newAnim = newAnimOverride.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
                    }

                    if (anim != newAnim) {
                        anim = newAnim;
                    }
                }

                if (anim != null)
                    animatorValidated = true;

                if (!animatorValidated)
                {

                    EditorGUILayout.HelpBox("This Component needs to be placed next to an Animator Component", MessageType.Error);
                    return;

                }

                GUILayout.BeginVertical(EditorStyles.helpBox);
                
                List<MLPASAnimatorSFX> validatedAnimatorSfxes = new List<MLPASAnimatorSFX>();

                foreach (var i in anim.GetBehaviours<MLPASAnimatorSFX>())
                {
                    if (!validatedAnimatorSfxes.Contains(i))
                 validatedAnimatorSfxes.Add(i);
                }

                Color color_default = GUI.backgroundColor;


                GUIStyle itemStyle = new GUIStyle(GUI.skin.box);  //make a new GUIStyle

                itemStyle.alignment = TextAnchor.MiddleLeft; //align text to the left
                itemStyle.active.background = itemStyle.normal.background;  //gets rid of button click background style.
                itemStyle.margin = new RectOffset(0, 0, 0, 0); //removes the space between items (previously there was a small gap between GUI which made it harder to select a desired item)
                itemStyle.font = EditorStyles.miniFont;
                itemStyle.fontSize = 10;
                itemStyle.fixedWidth = 0;
                itemStyle.stretchWidth = true;
                itemStyle.wordWrap = true;
                itemStyle.richText = true;
                itemStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color(0.8f, 0.8f, 0.8f) : Color.black;
                itemStyle.hover.textColor = itemStyle.normal.textColor;
                itemStyle.active.textColor = itemStyle.normal.textColor;
                itemStyle.focused.textColor = itemStyle.normal.textColor;
                itemStyle.normal.background = uiBack;
                itemStyle.hover.background = uiBack;
                itemStyle.active.background = uiBack;
                itemStyle.focused.background = uiBack;

                if (validatedAnimatorSfxes.Count > 0)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    for (int i = 0; i < validatedAnimatorSfxes.Count; i++)
                    {

                        // Color font_default = GUI.color;
                        GUI.backgroundColor = (selectedIndexStateM == i) ? color_selected : new Color(1, 1, 1, 0.25f);
                        if (EditorGUIUtility.isProSkin)
                        GUI.backgroundColor = (selectedIndexStateM == i) ? colorPro_selected : new Color(0.25f, 0.25f, 0.25f, 0.25f);
                        //  GUI.color = (selectedIndex == i) ? font_selected : font_default;

                        string layerName = "";

                        for (int iL = 0; iL < anim.layers.Length; iL++)
                        {
                            if (iL == validatedAnimatorSfxes[i].transitionLayer)
                            layerName = anim.layers[iL].name;
                        }

                        string buttonName = "L: " + layerName + " | " + (validatedAnimatorSfxes[i].currentState != null ? "S: " + validatedAnimatorSfxes[i].currentState.name : "SM: " + validatedAnimatorSfxes[i].currentStateMachine.name);


                        if (GUILayout.Button(buttonName, itemStyle))
                        {
                            selectedIndexStateM = i;

                            if (prevSelectedIndexStateM != selectedIndexStateM)
                            {
                                Repaint();
                                prevSelectedIndexStateM = selectedIndexStateM;
                                EditorGUI.FocusTextInControl(null);
                            }

                            valuesModified = true;
                        }

                        GUI.backgroundColor = color_default; //this is to avoid affecting other GUIs outside of the list


                    }



                    GUILayout.EndVertical();

                    if (EditorApplication.isPlaying)
                        GUI.enabled = false;



                    EditorGUILayout.Space();

                    MLPASAnimatorSFXController.ValuesOverride newValue = null;

                    if (!ContainsStateEditor(validatedAnimatorSfxes[selectedIndexStateM], out newValue))
                    {
                        newValue = new MLPASAnimatorSFXController.ValuesOverride();

                        UnityEditor.Animations.StateMachineBehaviourContext[] context = UnityEditor.Animations.AnimatorController.FindStateMachineBehaviourContext(validatedAnimatorSfxes[selectedIndexStateM]);
                        UnityEditor.Animations.AnimatorState cState = (context[0].animatorObject as UnityEditor.Animations.AnimatorState);
                        UnityEditor.Animations.AnimatorStateMachine cStateMachine = (context[0].animatorObject as UnityEditor.Animations.AnimatorStateMachine);

                        string stateName = cState != null ? cState.name : cStateMachine.name;
                        int layer = context[0].layerIndex;

                        newValue.layer = layer;
                        newValue.stateName = stateName;
                        (obj.targetObject as MLPASAnimatorSFXController).newValues.Add(newValue);
                    }
                    else
                    {

                        if (!validatedAnimatorSfxes[selectedIndexStateM].hasID)
                        {
                            validatedAnimatorSfxes[selectedIndexStateM].id = validatedAnimatorSfxes[selectedIndexStateM].GetInstanceID();
                            validatedAnimatorSfxes[selectedIndexStateM].hasID = true;
                            EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);

                            EditorUtility.SetDirty(anim);
                        }

                        if (!newValue.HASID)
                        {
                            newValue.HASID = true;
                            newValue.ID = validatedAnimatorSfxes[selectedIndexStateM].id;
                            valuesModified = true;
                        }
                    }


                    BoolField("Use Different Play Position", ref newValue.useDifferentPlayPosition);

                    if (newValue.useDifferentPlayPosition)
                    {

                        TransformField("Play Position Transform", ref newValue.playPosition);
                        BoolField("Follow Play Position", ref newValue.followPosition);

                    }
                    EditorGUILayout.Space();
                    BoolField("Override State values", ref newValue.overrideStateSFXsValues);

                    if (newValue.overrideStateSFXsValues)
                    {
                        for (int i = 0; i < validatedAnimatorSfxes[selectedIndexStateM].stateSfxs.Count; i++)
                        {

                            MLPASAnimatorSFX.StateSFX _stateSfx = validatedAnimatorSfxes[selectedIndexStateM].stateSfxs[i];

                            if (!_stateSfx.IDFinded)
                            {
                                _stateSfx.IDFinded = true;

                                _stateSfx.ID = validatedAnimatorSfxes[selectedIndexStateM].globalAddedSfx - i;

                                EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                            }

                            string _name = _stateSfx.customName;

                            if (string.IsNullOrEmpty(_name) && _stateSfx.audioObject != null)
                                _name = _stateSfx.audioObject.name;

                            if (string.IsNullOrEmpty(_name) && !_stateSfx.useIdentifier && !string.IsNullOrEmpty(_stateSfx.audioObjectIdentifier))
                                _name = "ID: " + _stateSfx.audioObjectIdentifier;

                            if (string.IsNullOrEmpty(_name) && _stateSfx.useCustomPlayMethod && !string.IsNullOrEmpty(_stateSfx.methodName))
                                _name = "M: " + _stateSfx.methodName;

                            if (string.IsNullOrEmpty(_name))
                                _name = "State SFX";

                            _name = i.ToString("D2") +"-" + _name;



                            if (newValue.HasStateValue(_stateSfx.ID))
                            {

                                MLPASAnimatorSFX.StateSFX.OverrideValues _values = newValue.GetStateValue(_stateSfx.ID);

                                bool _dirty = false;

                                if (_values.OverridePlayTime!= _stateSfx.cOverridePlayTime)
                                    _dirty = true;

                                _stateSfx.cOverridePlayTime = _values.OverridePlayTime;

                                if (_values.OverrideCustomParameters != _stateSfx.cOverrideCustomParameters)
                                    _dirty = true;

                                _stateSfx.cOverrideCustomParameters = _values.OverrideCustomParameters;

                                if (_values.OverrideAudioObject != _stateSfx.cOverrideAudioObject)
                                    _dirty = true;

                                _stateSfx.cOverrideAudioObject = _values.OverrideAudioObject;

                                if (_values.OverrideChannel != _stateSfx.cOverrideChannel)
                                    _dirty = true;

                                _stateSfx.cOverrideChannel = _values.OverrideChannel;

                                if (_values.OverrideListenerPause != _stateSfx.cOverrideListenerPause)
                                    _dirty = true;

                                _stateSfx.cOverrideListenerPause = _values.OverrideListenerPause;

                                if (_values.playTime != _stateSfx.cPlayTime)
                                    _dirty = true;

                                _stateSfx.cPlayTime = _values.playTime;

                                if (_values.customParams.BoolParameter != _stateSfx.cBoolParameter)
                                    _dirty = true;

                                _stateSfx.cBoolParameter = _values.customParams.BoolParameter;

                                if (_values.customParams.FloatParameter != _stateSfx.cFloatParameter)
                                    _dirty = true;

                                _stateSfx.cFloatParameter = _values.customParams.FloatParameter;

                                if (_values.customParams.IntParameter != _stateSfx.cIntParameter)
                                    _dirty = true;

                                _stateSfx.cIntParameter = _values.customParams.IntParameter;

                                if (_values.customParams.StringParameter != _stateSfx.cStringParameter)
                                    _dirty = true;

                                _stateSfx.cStringParameter = _values.customParams.StringParameter;

                                if (_values.customParams.ObjectParameter != _stateSfx.cObjectParameter)
                                    _dirty = true;

                                _stateSfx.cObjectParameter = _values.customParams.ObjectParameter;

                                if (_values.useIdentifier != _stateSfx.cUseIdentifier)
                                    _dirty = true;

                                _stateSfx.cUseIdentifier = _values.useIdentifier;

                                if (_values.identifier != _stateSfx.cIdentifier)
                                    _dirty = true;

                                _stateSfx.cIdentifier = _values.identifier;

                                if (_values.audioObject != _stateSfx.cAudioObject)
                                    _dirty = true;

                                _stateSfx.cAudioObject = _values.audioObject;

                                if (_values.useChannel != _stateSfx.cUseChannel)
                                    _dirty = true;

                                _stateSfx.cUseChannel = _values.useChannel;

                                if (_values.channel != _stateSfx.cChannel)
                                    _dirty = true;

                                _stateSfx.cChannel = _values.channel;

                                if (_values.ignoreListenerPause != _stateSfx.cIgnoreListenerPause)
                                    _dirty = true;

                                _stateSfx.cIgnoreListenerPause = _values.ignoreListenerPause;

                                if (_dirty)
                                {
                                    _dirty = false;
                                    EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                }

                            }
                            else
                            {
                                _stateSfx.cOverridePlayTime = false;
                                _stateSfx.cOverrideCustomParameters = false;
                                _stateSfx.cOverrideAudioObject = false;
                                _stateSfx.cOverrideChannel = false;
                                _stateSfx.cOverrideListenerPause = false;

                                _stateSfx.cPlayTime = -1;

                                _stateSfx.cBoolParameter = false;
                                _stateSfx.cFloatParameter = 0f;
                                _stateSfx.cIntParameter = 0;
                                _stateSfx.cStringParameter = string.Empty;
                                _stateSfx.cObjectParameter = null;

                                _stateSfx.cUseIdentifier = false;
                                _stateSfx.cIdentifier = string.Empty;
                                _stateSfx.cAudioObject = null;
                                _stateSfx.cUseChannel = false;
                                _stateSfx.cChannel = -1;
                                _stateSfx.cIgnoreListenerPause = false;

                             
                                    EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                 
                            }

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUI.BeginChangeCheck();
                            GUIStyle _foldoutStyle = new GUIStyle(EditorStyles.foldout);
                            _foldoutStyle.font = EditorStyles.boldFont;
                            _stateSfx.Show = GUILayout.Toggle(_stateSfx.Show,_name, _foldoutStyle);
                            if (EditorGUI.EndChangeCheck())
                            {
                                EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                            }

                           

                            if (_stateSfx.Show)
                            {
                                EditorGUILayout.Space();
                                if (_stateSfx.playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnUpdate || _stateSfx.playMode == MLPASAnimatorSFX.MLPASAnimatorPlayMode.OnTransitionTime)
                                {

                                    bool _prevBool = _stateSfx.cOverridePlayTime;
                                    bool _newBool = _prevBool;
                                    BoolField("Override Play time", ref _newBool);

                                    if (_prevBool != _newBool)
                                    {
                                        Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Override Play time");
                                        _stateSfx.cOverridePlayTime = _newBool;
                                        EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                        valuesModified = true;
                                    }

                                    if (_stateSfx.cOverridePlayTime)
                                    {
                                        if (GUILayout.Button("Get StateSFX Value", EditorStyles.miniButton))
                                        {
                                            Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Get StateSFX Value");
                                            Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), "Get StateSFX Value");
                                            _stateSfx.cPlayTime = _stateSfx.playTime;
                                            valuesModified = true;
                                            EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                        }
               
                                        float _prevFloat = _stateSfx.cPlayTime;
                                        float _newFloat = _prevFloat;
                                        FloatField("Play Time (%)", ref _newFloat);
                                        if (_prevFloat != _newFloat)
                                        {
                                            Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Play Time (%)");
                                            _stateSfx.cPlayTime = _newFloat;
                                            EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                            valuesModified = true;
                                        }

                                        EditorGUILayout.Space();
                                    }

                                }

                                bool _prevBool2 = _stateSfx.cOverrideAudioObject;
                                bool _newBool2 = _prevBool2;
                                BoolField("Override Audio Object", ref _newBool2);
                                if (_prevBool2 != _newBool2)
                                {
                                    Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Override Audio Object");
                                    _stateSfx.cOverrideAudioObject = _newBool2;
                                    EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                    valuesModified = true;
                                }

                                if (_stateSfx.cOverrideAudioObject)
                                {
                                    if (GUILayout.Button("Get StateSFX Value", EditorStyles.miniButton))
                                    {
                                        Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Get StateSFX Value");
                                        Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), "Get StateSFX Value");
                                        _stateSfx.cUseIdentifier =  _stateSfx.useIdentifier;
                                        if (_stateSfx.useIdentifier)
                                        {
                                            _stateSfx.cIdentifier = _stateSfx.audioObjectIdentifier;
                                        }
                                        else
                                        {
                                            _stateSfx.cAudioObject = _stateSfx.audioObject;
                                        }

                                        valuesModified = true;
                                        EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                    }

                                    bool _prevBool3 = _stateSfx.cUseIdentifier;
                                    bool _newBool3 = _prevBool3;
                                    BoolField("Use Identifier", ref _newBool3, true);
                                    if (_prevBool3 != _newBool3)
                                    {
                                        Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Use Identifier");
                                        _stateSfx.cUseIdentifier = _newBool3;
                                        EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                        valuesModified = true;
                                    }

                                    if (_stateSfx.cUseIdentifier)
                                    {
                                        string _prevString = _stateSfx.cIdentifier;
                                        string _newString = _prevString;
                                        StringField("AO Identifier", ref _newString);
                                        if (_prevString != _newString)
                                        {
                                            Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "AO Identifier");
                                            _stateSfx.cIdentifier = _newString;
                                            EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                            valuesModified = true;
                                        }

                                    }
                                    else
                                    {
                                        AudioObject _prevAudioObject = _stateSfx.cAudioObject;
                                        AudioObject _newAudioObject = _prevAudioObject;
                                        AudioObjectField("Audio Object", ref _newAudioObject);
                                        if (_prevAudioObject != _newAudioObject)
                                        {
                                            Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Audio Object");
                                            _stateSfx.cAudioObject = _newAudioObject;
                                            EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                            valuesModified = true;
                                        }

                                    }

                                    EditorGUILayout.Space();
                                }

                                bool _prevBool4 = _stateSfx.cOverrideChannel;
                                bool _newBool4 = _prevBool4;
                                BoolField("Override Channel", ref _newBool4);
                                if (_prevBool4 != _newBool4)
                                {
                                    Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Override Channel");
                                    _stateSfx.cOverrideChannel = _newBool4;
                                    EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                    valuesModified = true;
                                }

                                if (_stateSfx.cOverrideChannel)
                                {
                                    if (GUILayout.Button("Get StateSFX Value", EditorStyles.miniButton))
                                    {
                                        Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Get StateSFX Value");
                                        Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), "Get StateSFX Value");
                                        _stateSfx.cUseChannel = _stateSfx.useChannel;

                                        if (_stateSfx.useChannel)
                                            _stateSfx.cChannel = _stateSfx.channel;

                                        valuesModified = true;
                                        EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                    }

                                    bool _prevBool5 = _stateSfx.cUseChannel;
                                    bool _newBool5 = _prevBool5;
                                    BoolField("Use Channel", ref _newBool5, true);
                                    if (_prevBool5 != _newBool5)
                                    {
                                        Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Use Channel");
                                        _stateSfx.cUseChannel = _newBool5;
                                        EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                        valuesModified = true;
                                    }

                                    if (_stateSfx.cUseChannel)
                                    {
                                        int _prevInt = _stateSfx.cChannel;
                                        int _newInt = _prevInt;
                                        IntField("Channel", ref _newInt);
                                        if (_prevInt != _newInt)
                                        {
                                            Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Channel");
                                            _stateSfx.cChannel = _newInt;
                                            EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                            valuesModified = true;
                                        }
                                    }
 
                                    EditorGUILayout.Space();
                                }
                                bool _prevBool6 = _stateSfx.cOverrideListenerPause;
                                bool _newBool6 = _prevBool6;
                                    BoolField("Override Listener Pause", ref _newBool6);
                                if (_prevBool6 != _newBool6)
                                {
                                    Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Override Listener Pause");
                                    _stateSfx.cOverrideListenerPause = _newBool6;
                                    EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                    valuesModified = true;
                                }

                                if (_stateSfx.cOverrideListenerPause)
                                {
                                    if (GUILayout.Button("Get StateSFX Value", EditorStyles.miniButton))
                                    {
                                        Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Get StateSFX Value");
                                        Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), "Get StateSFX Value");
                                        _stateSfx.cIgnoreListenerPause = _stateSfx.IgnoreListenerPause;
                                        valuesModified = true;
                                        EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                    }

                                    bool _prevBool7 = _stateSfx.cIgnoreListenerPause;
                                    bool _newBool7 = _prevBool7;
                                        BoolField("Ignore Listener Pause", ref _newBool7, true);
                                    if (_prevBool7 != _newBool7)
                                    {
                                        Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Ignore Listener Pause");
                                        _stateSfx.cIgnoreListenerPause = _newBool7;
                                        EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                        valuesModified = true;
                                    }


                                    EditorGUILayout.Space();
                                }

                                bool _prevBool8 = _stateSfx.cOverrideCustomParameters;
                                bool _newBool8 = _prevBool8;
                                    BoolField("Override Custom Parameters", ref _newBool8);
                                if (_prevBool8 != _newBool8)
                                {
                                    Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Override Custom Parameters");
                                    _stateSfx.cOverrideCustomParameters = _newBool8;
                                    EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                    valuesModified = true;
                                }


                                if (_stateSfx.cOverrideCustomParameters)
                                {

                                    if (GUILayout.Button("Get StateSFX Value", EditorStyles.miniButton))
                                    {
                                        Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Get StateSFX Value");
                                        Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), "Get StateSFX Value");
                                        _stateSfx.cBoolParameter = _stateSfx.userParameters.BoolParameter;
                                        _stateSfx.cFloatParameter = _stateSfx.userParameters.FloatParameter;
                                        _stateSfx.cIntParameter = _stateSfx.userParameters.IntParameter;
                                        _stateSfx.cStringParameter = _stateSfx.userParameters.StringParameter;
                                        _stateSfx.cObjectParameter = _stateSfx.userParameters.ObjectParameter;
                                        valuesModified = true;
                                        EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                    }

                                    bool _boolParam = _stateSfx.cBoolParameter;
                                    float _floatParam = _stateSfx.cFloatParameter;
                                    int _intParam = _stateSfx.cIntParameter;
                                    string _stringParam = _stateSfx.cStringParameter;
                                    Object _objectParam = _stateSfx.cObjectParameter;

                                    bool _boolParamNew = _boolParam;
                                    float _floatParamNew = _floatParam;
                                    int _intParamNew = _intParam;
                                    string _stringParamNew = _stringParam;
                                    Object _objectParamNew = _objectParam;

                                    CustomParametersField(ref _boolParamNew, ref _floatParamNew, ref _intParamNew, ref _stringParamNew, ref _objectParamNew);

                                    if (_boolParam!= _boolParamNew || _floatParam != _floatParamNew ||
                                        _intParam != _intParamNew || _stringParam != _stringParamNew ||
                                        _objectParam != _objectParamNew)
                                    {
                                        Undo.RecordObject(validatedAnimatorSfxes[selectedIndexStateM], "Custom Parameters");
                                        _stateSfx.cBoolParameter = _boolParamNew;
                                        _stateSfx.cFloatParameter = _floatParamNew;
                                        _stateSfx.cIntParameter = _intParamNew;
                                        _stateSfx.cStringParameter = _stringParamNew;
                                        _stateSfx.cObjectParameter = _objectParamNew;
                                        EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);
                                        valuesModified = true;
                                    }

                                }
                                

                            }

                            if (valuesModified)
                            {

                                MLPASAnimatorSFX.StateSFX.OverrideValues _values = new MLPASAnimatorSFX.StateSFX.OverrideValues();

                                _values.OverridePlayTime = _stateSfx.cOverridePlayTime;
                                _values.OverrideCustomParameters = _stateSfx.cOverrideCustomParameters;
                                _values.OverrideAudioObject = _stateSfx.cOverrideAudioObject;
                                _values.OverrideChannel = _stateSfx.cOverrideChannel;
                                _values.OverrideListenerPause = _stateSfx.cOverrideListenerPause;

                                _values.playTime = _stateSfx.cPlayTime;
                                _values.customParams = new MLPASACustomPlayMethodParameters.CustomParams(_stateSfx.cBoolParameter, _stateSfx.cFloatParameter, _stateSfx.cIntParameter, _stateSfx.cStringParameter, _stateSfx.cObjectParameter);
                                _values.useIdentifier = _stateSfx.cUseIdentifier;
                                _values.identifier = _stateSfx.cIdentifier;
                                _values.audioObject = _stateSfx.cAudioObject;
                                _values.useChannel = _stateSfx.cUseChannel;
                                _values.channel = _stateSfx.cChannel;
                                _values.ignoreListenerPause = _stateSfx.cIgnoreListenerPause;

                                 newValue.AddStateValue(_stateSfx.ID, _values);
                             
                                 EditorUtility.SetDirty(validatedAnimatorSfxes[selectedIndexStateM]);

                            }

                            EditorGUILayout.EndVertical();

                        }

                    }


                    GUILayout.EndVertical();

                    if (!EditorApplication.isPlaying)
                    {

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Custom Play Methods", EditorStyles.boldLabel);

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                        customPlayMethodObj = EditorGUILayout.ObjectField(new GUIContent("Target GameObject", ""), customPlayMethodObj, typeof(GameObject), true) as GameObject;
                        if (customPlayMethodObj!=null && EditorUtility.IsPersistent(customPlayMethodObj))
                        {
                            customPlayMethodObj = null;
                        }
                        List<MLPASAnimatorSFXController.InspectorDelegate> inspectorDelegates = new List<MLPASAnimatorSFXController.InspectorDelegate>();



                        if (customPlayMethodObj != null)
                        {

                            Component[] components = customPlayMethodObj.GetComponents<Component>();

                            foreach (var item in components)
                            {

                                System.Reflection.MethodInfo[] methods = item.GetType().GetMethods();


                                for (int i = 0; i < methods.Length; i++)
                                {
                                    System.Reflection.ParameterInfo[] parameters = methods[i].GetParameters();

                                    for (int i2 = 0; i2 < parameters.Length; i2++)
                                    {

                                        if (parameters[i2].ParameterType == typeof(MLPASACustomPlayMethodParameters))
                                        {
                                            MLPASAnimatorSFXController.InspectorDelegate del = new MLPASAnimatorSFXController.InspectorDelegate();
                                            del.methodName = methods[i].Name;
                                            del.target = item;
                                            inspectorDelegates.Add(del);
                                            break;
                                        }


                                    }
                                }

                            }


                            string[] methodNames = new string[inspectorDelegates.Count];


                            for (int i = 0; i < inspectorDelegates.Count; i++)
                            {
                                methodNames[i] = i.ToString()+" - " + inspectorDelegates[i].methodName + " (MLPASACustomPlayMethodParameters)";

                            }

                            if (inspectorDelegates.Count > 0)
                            {

                                playMethodIndex = EditorGUILayout.Popup(playMethodIndex, methodNames);

                                bool alreadyExists = false;

                                foreach (var item in (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates)
                                {

                                    if (item.methodName == inspectorDelegates[playMethodIndex].methodName)
                                    {
                                        alreadyExists = true;
                                        break;
                                    }
                                }

                                bool prevGuiEnabled = GUI.enabled;
                                GUI.enabled = !alreadyExists;
                                Color prevBackground = GUI.backgroundColor;
                                GUI.backgroundColor = new Color(0.35f, 0.8f, 0.95f);
                                if (GUILayout.Button(alreadyExists ? inspectorDelegates[playMethodIndex].methodName + " Already Exists" : "Add Custom Play Method", EditorStyles.miniButton))
                                {
                                    if ((obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates == null)
                                        (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates = new List<MLPASAnimatorSFXController.InspectorDelegate>();

                                    (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.Add(inspectorDelegates[playMethodIndex]);

                                    valuesModified = true;
                                }
                                GUI.backgroundColor = prevBackground;
                                GUI.enabled = prevGuiEnabled;
                            }
                            else
                            {
                                playMethodIndex = 0;

                                EditorGUILayout.LabelField("No methods found", EditorStyles.miniLabel);
                                bool prevGuiEnabled = GUI.enabled;
                                GUI.enabled = false;
                                Color prevBackground = GUI.backgroundColor;
                                GUI.backgroundColor = new Color(0.35f, 0.8f, 0.95f);
                                GUILayout.Button("Select Another GameObject", EditorStyles.miniButton);
                                GUI.backgroundColor = prevBackground;
                                GUI.enabled = prevGuiEnabled;


                            }


                        }
                        else
                        {
                            bool prevGuiEnabled = GUI.enabled;
                            GUI.enabled = false;
                            Color prevBackground = GUI.backgroundColor;
                            GUI.backgroundColor = new Color(0.35f, 0.8f, 0.95f);
                            GUILayout.Button("Select a GameObject", EditorStyles.miniButton);
                            GUI.backgroundColor = prevBackground;
                            GUI.enabled = prevGuiEnabled;
                        }

                        EditorGUILayout.EndVertical();

                        for (int i = 0; i < (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.Count; i++)
                        {

                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            bool prevGuiEnabled = GUI.enabled;
                            GUI.enabled = false;
                            if ((obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].target == null)
                            {
                                EditorGUILayout.ObjectField("Target GameObject", null, typeof(GameObject), true);
                            }
                            else
                            {
                                EditorGUILayout.ObjectField("Target GameObject", (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].target.gameObject, typeof(GameObject), true);
                            }

                            if (!(obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].removed)
                                EditorGUILayout.Popup(0, new string[] { (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].methodName + " (MLPASACustomPlayMethodParameters)" });
                            else
                                EditorGUILayout.Popup(0, new string[] { (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates[i].methodName + " | MISSING" });

                            GUI.enabled = prevGuiEnabled;

                            Color prevBackground = GUI.backgroundColor;
                            GUI.backgroundColor = new Color(1f, 0.35f, 0.38f);
                            if (GUILayout.Button("Remove Play Method", EditorStyles.miniButton))
                            {
                                (obj.targetObject as MLPASAnimatorSFXController).inspectorDelegates.RemoveAt(i);
                                valuesModified = true;
                            }
                            GUI.backgroundColor = prevBackground;

                            EditorGUILayout.EndVertical();
                        }



                    }


                    if (valuesModified && !dirty)
                    {

                        dirty = true;
                        valuesModified = false;
                        if (!EditorApplication.isPlaying)
                        {
                            (obj.targetObject as MLPASAnimatorSFXController).newValues[(obj.targetObject as MLPASAnimatorSFXController).newValues.FindIndex(x => x.ID == newValue.ID)] = newValue;
                            SetObjectDirty((obj.targetObject as MLPASAnimatorSFXController));
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("The 'Animator' Next to this 'MLPASAnimatorSFXController' doesn't have any 'MLPASAnimatorSFX' State Machine Behaviour", MessageType.Warning);
                    GUILayout.EndVertical();

                    return;
                }



                GUI.enabled = true;


                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.Space();

                    bool nullMethods = true;
                    EditorGUILayout.LabelField("Registered Custom Play Methods", EditorStyles.boldLabel);

                    foreach (var item in (obj.targetObject as MLPASAnimatorSFXController).customPlayMethods)
                    {
                        if (item.Value != null)
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            GUI.enabled = false;
                            Component comp = ((item.Value.Target) as Component);
                            if (comp != null)
                                nullMethods = false;

                            EditorGUILayout.ObjectField("Target GameObject", comp != null ? comp.gameObject : null, typeof(GameObject), true);

                            if (comp != null)
                            {
                                EditorGUILayout.Popup(0, new string[] { item.Value.Method.Name + " (MLPASACustomPlayMethodParameters)" });
                            }
                            else
                            {
                                EditorGUILayout.Popup(0, new string[] { item.Value.Method.Name + " | MISSING" });
                            }
                            GUI.enabled = true;
                            EditorGUILayout.EndVertical();
                        }
                    }

                    if (nullMethods)
                    {
                        EditorGUILayout.LabelField("No methods found");
                    }
                }

                obj.ApplyModifiedProperties();

            }

            void GameObjectField(string controlName, ref GameObject value)
            {
                EditorGUI.BeginChangeCheck();
                GameObject newvalue = EditorGUILayout.ObjectField(new GUIContent(controlName), value, typeof(GameObject), false) as GameObject;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(obj.targetObject, controlName);
                    value = newvalue;
                    valuesModified = true;
                }
            }

            public void SetObjectDirty(Component comp)
            {
                EditorUtility.SetDirty(comp);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(comp.gameObject.scene); //This used to happen automatically from SetDirty
            }

            bool BoolField(string controlName, ref bool value, bool left=false)
            {
                EditorGUI.BeginChangeCheck();
                bool newBool = !left ? EditorGUILayout.Toggle(controlName, value) : EditorGUILayout.ToggleLeft(controlName, value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), controlName);
                    value = newBool;
                    valuesModified = true;
                }
                return value;

            }

            void TransformField(string controlName, ref Transform value)
            {

                EditorGUI.BeginChangeCheck();
                Transform newTransform = EditorGUILayout.ObjectField(new GUIContent(controlName), value, typeof(Transform), true) as Transform;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), controlName);
                    value = newTransform;
                    valuesModified = true;
                }
            }

            int IntField(string controlName, ref int value)
            {
                EditorGUI.BeginChangeCheck();
                int newValue = EditorGUILayout.IntField(controlName, value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), controlName);
                    value = newValue;
                    valuesModified = true;
                }
                return value;

            }

            float FloatField(string controlName, ref float value, float min = 0f, float max = 100f)
            {
                EditorGUI.BeginChangeCheck();
                float newvalue = Mathf.Clamp(EditorGUILayout.Slider(new GUIContent(controlName), Mathf.Clamp(value, min, max), min, max), min, max);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), controlName);
                    value = newvalue;
                    valuesModified = true;
                }
                return newvalue;
            }

            string StringField(string controlName, ref string value)
            {
                EditorGUI.BeginChangeCheck();
                string newValue = EditorGUILayout.TextField(controlName, value);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), controlName);
                    value = newValue;
                    valuesModified = true;
                }
                return value;

            }

            AudioObject AudioObjectField(string controlName, ref AudioObject value)
            {
                EditorGUI.BeginChangeCheck();
                AudioObject newValue = EditorGUILayout.ObjectField(new GUIContent(controlName), value, typeof(AudioObject), true) as AudioObject;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), controlName);
                    value = newValue;
                    valuesModified = true;
                }
                return value;

            }

            void CustomParametersField(ref bool _boolValue, ref float _floatValue, ref int _intValue, ref string _stringValue, ref Object _objectValue)
            {
                EditorGUI.BeginChangeCheck();
                bool boolParameter = EditorGUILayout.Toggle("Bool Parameter", _boolValue);
                float floatParameter = EditorGUILayout.FloatField("Float Parameter", _floatValue);
                int intParameter = EditorGUILayout.IntField("Int Parameter", _intValue);
                string stringParameter = EditorGUILayout.TextField("String Parameter", _stringValue);
                Object objectParameter = EditorGUILayout.ObjectField(new GUIContent("Object Parameter"), _objectValue, typeof(Object), true) as Object;


                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject((obj.targetObject as MLPASAnimatorSFXController), "Custom Parameters Override");
                    _boolValue = boolParameter;
                    _floatValue = floatParameter;
                    _intValue = intParameter;
                    _stringValue = stringParameter;
                    _objectValue = objectParameter;
                    valuesModified = true;
                }

            }


        }
#endif
    }
}