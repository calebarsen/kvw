// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Level)]
	[Tooltip("Loads a Level by Index number. Before you can load a level, you have to add it to the list of levels defined in File->Build Settings...")]
	public class LoadLevelNum : FsmStateAction
	{
		[RequiredField]
        [Tooltip("The level index in File->Build Settings")]
		public FsmInt levelIndex;
		
        [Tooltip("Load the level additively, keeping the current scene.")]
        public bool additive;

        [Tooltip("Event to send after the level is loaded.")]
		public FsmEvent loadedEvent;

        [Tooltip("Don't destroy this FSM when loading the new level.")]
		public FsmBool dontDestroyOnLoad;

		public override void Reset()
		{
			levelIndex = null;
			additive = false;
			loadedEvent = null;
			dontDestroyOnLoad = false;
		}

		public override void OnEnter()
		{
			if (dontDestroyOnLoad.Value)
			{
				// Have to get the root, since this FSM will be destroyed if a parent is destroyed.
				
				var root = Owner.transform.root;
				Object.DontDestroyOnLoad(root.gameObject);
			}

			if (additive)
			{
			    Application.LoadLevelAdditive(levelIndex.Value);
			}
			else
			{
			    Application.LoadLevel(levelIndex.Value);
			}

			Fsm.Event(loadedEvent);
			Finish();
		}
	}
}