using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Photon.Pun;
using UnityEngine;

public class DebugCommandHandler : MonoBehaviour
{
	public class ChatCommand
	{
		public string Name { get; }

		public string Description { get; }

		public Action<bool, string[]> Execute { get; }

		public Func<bool, string, string[], List<string>> Suggest { get; }

		public Func<bool> IsEnabled { get; }

		public bool IsDebug { get; }

		public ChatCommand(string name, string description, Action<bool, string[]> execute, Func<bool, string, string[], List<string>> suggest = null, Func<bool> isEnabled = null, bool debugOnly = true)
		{
			Name = name;
			Description = description;
			Execute = execute;
			Suggest = suggest ?? new Func<bool, string, string[], List<string>>(DefaultSuggest);
			IsEnabled = isEnabled ?? ((Func<bool>)(() => true));
			IsDebug = debugOnly;
		}

		private List<string> DefaultSuggest(bool isDebugConsole, string partial, string[] args)
		{
			return new List<string>();
		}
	}

	public static DebugCommandHandler instance;

	internal readonly Dictionary<string, ChatCommand> _commands = new Dictionary<string, ChatCommand>();

	internal string lastExecutedCommand;

	private Level moduleLevel;

	private PrefabRef moduleStartRoom;

	private PrefabRef moduleObject;

	private Module.Type moduleType;

	internal bool debugOverlay = true;

	internal bool infiniteEnergy;

	internal bool godMode;

	internal bool enemyNoVision;

	public void Register(ChatCommand cmd)
	{
		if (cmd == null)
		{
			throw new ArgumentNullException("cmd");
		}
		_commands[cmd.Name] = cmd;
	}

	internal bool Execute(string input, bool isDebugConsole = false)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return false;
		}
		string[] array = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return false;
		}
		string[] arg = array.Skip(1).ToArray();
		if (!_commands.TryGetValue(array[0], out var value) || !value.IsEnabled() || (!isDebugConsole && value.IsDebug))
		{
			return false;
		}
		try
		{
			value.Execute?.Invoke(isDebugConsole, arg);
			lastExecutedCommand = input;
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return true;
	}

	internal List<string> GetSuggestions(string partial, bool isDebugConsole = false)
	{
		if (partial == null)
		{
			partial = string.Empty;
		}
		string[] tokens = partial.Split(' ');
		switch (tokens.Length)
		{
		case 0:
			return (from kvp in _commands
				where kvp.Value.IsEnabled() && (isDebugConsole || !kvp.Value.IsDebug)
				orderby kvp.Value.IsDebug, kvp.Key
				select kvp.Key).ToList();
		case 1:
			return (from kvp in _commands
				where kvp.Value.IsEnabled() && (isDebugConsole || !kvp.Value.IsDebug) && kvp.Key.StartsWith(tokens[0])
				orderby kvp.Value.IsDebug, kvp.Key
				select kvp.Key).ToList();
		default:
		{
			string[] array = tokens.Skip(1).ToArray();
			if (!_commands.TryGetValue(tokens[0], out var value) || !value.IsEnabled() || (!isDebugConsole && value.IsDebug))
			{
				return new List<string>();
			}
			string arg = ((array.Length != 0) ? array[^1] : string.Empty);
			return value.Suggest?.Invoke(isDebugConsole, arg, array) ?? new List<string>();
		}
		}
	}

	private void CommandSuccessEffect()
	{
		MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Confirm, null, 1f, 1f, soundOnly: true);
	}

	private void CommandFailedEffect()
	{
		MenuManager.instance?.MenuEffectClick(MenuManager.MenuClickEffectType.Deny, null, 1f, 1f, soundOnly: true);
	}

	private bool IsInGame()
	{
		if (!SemiFunc.IsSplashScreen() && !SemiFunc.IsMainMenu())
		{
			return !SemiFunc.RunIsTutorial();
		}
		return false;
	}

	private void Awake()
	{
		debugOverlay = Debug.isDebugBuild;
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Register(new ChatCommand("cinematic", "Disables the hud", delegate
		{
			GameDirector.instance.CommandRecordingDirectorToggle();
			CommandSuccessEffect();
		}, null, () => !SemiFunc.RunIsLobbyMenu() && IsInGame(), debugOnly: false));
		Register(new ChatCommand("greenscreen", "Toggles a green screen", delegate
		{
			GameDirector.instance.CommandGreenScreenToggle();
			CommandSuccessEffect();
		}, null, () => !SemiFunc.RunIsLobbyMenu() && IsInGame(), debugOnly: false));
		Register(new ChatCommand("help", "List all enabled commands", delegate(bool isDebugConsole, string[] args)
		{
			List<ChatCommand> list = (from chatCommand2 in _commands.Values
				where isDebugConsole || !chatCommand2.IsDebug
				orderby chatCommand2.IsDebug, chatCommand2.Name
				select chatCommand2).ToList();
			if (args.Length == 0)
			{
				List<ChatCommand> list2 = list.Where((ChatCommand x) => x.IsEnabled()).ToList();
				if (list2.Count > 0)
				{
					Debug.Log("Enabled Commands:");
					foreach (ChatCommand item2 in list2)
					{
						Debug.Log(item2.Name + " - " + item2.Description);
					}
				}
				List<ChatCommand> list3 = list.Where((ChatCommand x) => !x.IsEnabled()).ToList();
				if (list3.Count > 0)
				{
					Debug.Log("Disabled Commands:");
					foreach (ChatCommand item3 in list3)
					{
						Debug.Log(item3.Name + " - " + item3.Description);
					}
				}
				DebugConsoleUI.instance?.SetResponseText($"Check the Player.log file for the list of commands\nEnabled Commands: <color=green>{list2.Count}/{list.Count}</color>", Color.white);
				CommandSuccessEffect();
			}
			else
			{
				ChatCommand chatCommand = list.FirstOrDefault((ChatCommand chatCommand2) => chatCommand2.Name == args[0]);
				if (chatCommand == null)
				{
					DebugConsoleUI.instance?.SetResponseText("No command named '" + args[0] + "'.", Color.red);
					CommandFailedEffect();
				}
				else
				{
					Debug.Log(chatCommand.Name + ": " + chatCommand.Description);
					DebugConsoleUI.instance?.SetResponseText("<color=green>" + chatCommand.Name + "</color>: " + chatCommand.Description, Color.white);
					CommandSuccessEffect();
				}
			}
		}, (bool isDebugConsole, string partial, string[] args) => (args.Length > 1) ? new List<string>() : (from chatCommand in _commands.Values
			where chatCommand.IsEnabled() && (isDebugConsole || !chatCommand.IsDebug)
			orderby chatCommand.IsDebug, chatCommand.Name
			select chatCommand.Name into n
			where n.StartsWith(partial)
			select n).ToList(), null, debugOnly: false));
		Register(new ChatCommand("logs", "Opens the Player.log file", delegate
		{
			string text = Application.persistentDataPath + "/Player.log";
			if (File.Exists(text))
			{
				SemiFunc.OpenFile(text);
			}
			else
			{
				Debug.LogWarning("Log file not found at: " + text);
			}
			CommandSuccessEffect();
		}, null, null, debugOnly: false));
		Register(new ChatCommand("saves", "Opens the saves folder", delegate
		{
			string text = Application.persistentDataPath + "/saves/";
			if (Directory.Exists(text))
			{
				SemiFunc.OpenFile(text);
			}
			else
			{
				Debug.LogWarning("Saves folder not found at: " + text);
			}
			CommandSuccessEffect();
		}, null, null, debugOnly: false));
		if (SemiFunc.DebugTester())
		{
			Register(new ChatCommand("clear", "Clears the developer console", delegate
			{
				Debug.ClearDeveloperConsole();
				CommandSuccessEffect();
			}));
			Register(new ChatCommand("cash", "Set current cash amount", delegate(bool isDebugConsole, string[] args)
			{
				if (args.Length == 0 || !int.TryParse(args[0], out var result))
				{
					CommandFailedEffect();
				}
				else
				{
					SemiFunc.StatSetRunCurrency(result);
					if (SemiFunc.RunIsShop())
					{
						RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false, RunManager.ChangeLevelType.Shop);
					}
					else if (SemiFunc.RunIsLevel() && (RoundDirector.instance?.extractionPointCurrent?.currentState).GetValueOrDefault() > ExtractionPoint.State.Idle)
					{
						RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false);
					}
					Debug.Log($"Command Used: /cash {result}");
					CommandSuccessEffect();
				}
			}, (bool isDebugConsole, string partial, string[] args) => (args.Length > 1) ? new List<string>() : new List<string> { "0", "5", "10", "50", "100" }.Where((string g) => args.Length == 0 || g.StartsWith(args[0])).ToList(), () => SemiFunc.IsMasterClientOrSingleplayer() && IsInGame()));
			Register(new ChatCommand("despawnenemies", "Despawn all enemies", delegate
			{
				int num = 0;
				foreach (EnemyParent item4 in EnemyDirector.instance.enemiesSpawned.ToList())
				{
					if ((bool)item4 && item4.Spawned)
					{
						num++;
						item4.SpawnedTimerSet(0f);
						item4.DespawnedTimerSet(10f);
					}
				}
				Debug.Log("Command Used: /despawnenemies");
				CommandSuccessEffect();
				DebugConsoleUI.instance?.SetResponseText($"Enemies Despawned: <color=green>{num}</color>", Color.white);
			}, null, () => SemiFunc.IsMasterClientOrSingleplayer() && !SemiFunc.RunIsLobbyMenu() && IsInGame()));
			Register(new ChatCommand("level", "Set current level scene", delegate(bool isDebugConsole, string[] args)
			{
				string levelName = string.Join(' ', args).ToLower();
				switch (levelName)
				{
				case "lobby menu":
					CommandFailedEffect();
					break;
				case "next":
					RunManager.instance.ChangeLevel(_completedLevel: true, SemiFunc.RunIsArena());
					break;
				case "random":
					RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false, RunManager.ChangeLevelType.RunLevel);
					break;
				case "recording":
					RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false, RunManager.ChangeLevelType.Recording);
					break;
				case "refresh":
					RunManager.instance.RestartScene();
					if (!SemiFunc.RunIsTutorial())
					{
						SemiFunc.OnSceneSwitch(RunManager.instance.gameOver, _leaveGame: false);
					}
					break;
				case "shop":
					RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false, RunManager.ChangeLevelType.Shop);
					break;
				default:
					if (levelName == "arena")
					{
						RunManager.instance.debugLevel = RunManager.instance.levelArena;
					}
					else if (levelName == "lobby")
					{
						RunManager.instance.debugLevel = RunManager.instance.levelLobby;
					}
					else
					{
						RunManager.instance.debugLevel = RunManager.instance.levels.FirstOrDefault((Level x) => Regex.Replace(x.name, "^Level - ", "").ToLower() == levelName);
						if (!RunManager.instance.debugLevel)
						{
							CommandFailedEffect();
							return;
						}
					}
					RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false);
					RunManager.instance.debugLevel = null;
					break;
				}
				Debug.Log("Command Used: /level " + levelName);
				CommandSuccessEffect();
			}, (bool isDebugConsole, string partial, string[] args) => (from g in (from x in RunManager.instance.levels
					select Regex.Replace(x.name, "^Level - ", "").ToLower() into x
					orderby x
					select x).Concat(new List<string> { "arena", "lobby", "recording", "shop" }).Prepend("random").Prepend("refresh")
					.Prepend("next")
				where args.Length == 0 || g.StartsWith(args[0].ToLower())
				select g).ToList(), () => SemiFunc.IsMasterClientOrSingleplayer() && IsInGame()));
			Register(new ChatCommand("levelnum", "Set current level number", delegate(bool isDebugConsole, string[] args)
			{
				if (args.Length == 0 || (!int.TryParse(args[0], out var result) && result > 0))
				{
					CommandFailedEffect();
				}
				else
				{
					RunManager.instance.levelsCompleted = result - 1;
					SemiFunc.StatSetRunLevel(RunManager.instance.levelsCompleted);
					if (SemiFunc.RunIsShop())
					{
						RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false, RunManager.ChangeLevelType.Shop);
					}
					else if (SemiFunc.RunIsLevel())
					{
						RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false);
					}
					Debug.Log($"Command Used: /level {result}");
					CommandSuccessEffect();
				}
			}, (bool isDebugConsole, string partial, string[] args) => (args.Length > 1) ? new List<string>() : new List<string> { "1", "5", "10", "15", "20" }.Where((string g) => args.Length == 0 || g.StartsWith(args[0])).ToList(), () => SemiFunc.IsMasterClientOrSingleplayer() && IsInGame()));
			Register(new ChatCommand("module", "Spawn a specific module", delegate(bool isDebugConsole, string[] args)
			{
				string typedRest = string.Join(' ', args).ToLower();
				PrefabRef prefabRef = (from x in RunManager.instance.levelCurrent.ModulesNormal1.Concat(RunManager.instance.levelCurrent.ModulesNormal2).Concat(RunManager.instance.levelCurrent.ModulesNormal3).Concat(RunManager.instance.levelCurrent.ModulesPassage1)
						.Concat(RunManager.instance.levelCurrent.ModulesPassage2)
						.Concat(RunManager.instance.levelCurrent.ModulesPassage3)
						.Concat(RunManager.instance.levelCurrent.ModulesDeadEnd1)
						.Concat(RunManager.instance.levelCurrent.ModulesDeadEnd2)
						.Concat(RunManager.instance.levelCurrent.ModulesDeadEnd3)
						.Concat(RunManager.instance.levelCurrent.ModulesExtraction1)
						.Concat(RunManager.instance.levelCurrent.ModulesExtraction2)
						.Concat(RunManager.instance.levelCurrent.ModulesExtraction3)
					where x != null
					select x).FirstOrDefault((PrefabRef x) => string.Join(" - ", x.PrefabName.Split(" - ").Skip(2)).ToLower() == typedRest);
				if (prefabRef != null)
				{
					moduleType = Module.Type.Normal;
					string[] array = prefabRef.PrefabName.ToLower().Split(" - ");
					if (array.Length > 2)
					{
						switch (array[2])
						{
						case "p":
							moduleType = Module.Type.Passage;
							break;
						case "de":
							moduleType = Module.Type.DeadEnd;
							break;
						case "e":
							moduleType = Module.Type.Extraction;
							break;
						}
					}
					moduleLevel = RunManager.instance.levelCurrent;
					moduleObject = prefabRef;
					RunManager.instance.RestartScene();
					if (!SemiFunc.RunIsTutorial())
					{
						SemiFunc.OnSceneSwitch(RunManager.instance.gameOver, _leaveGame: false);
					}
					Debug.Log("Command Used: /module " + typedRest);
				}
				else
				{
					moduleObject = null;
				}
				CommandSuccessEffect();
				DebugConsoleUI.instance?.SetResponseText((prefabRef != null) ? "Module Override: <color=green>Enabled</color>" : "Module Override: <color=red>Disabled</color>", Color.white);
			}, (bool isDebugConsole, string partial, string[] args) => (from x in (from x in RunManager.instance.levelCurrent.ModulesNormal1.Concat(RunManager.instance.levelCurrent.ModulesNormal2).Concat(RunManager.instance.levelCurrent.ModulesNormal3).Concat(RunManager.instance.levelCurrent.ModulesPassage1)
						.Concat(RunManager.instance.levelCurrent.ModulesPassage2)
						.Concat(RunManager.instance.levelCurrent.ModulesPassage3)
						.Concat(RunManager.instance.levelCurrent.ModulesDeadEnd1)
						.Concat(RunManager.instance.levelCurrent.ModulesDeadEnd2)
						.Concat(RunManager.instance.levelCurrent.ModulesDeadEnd3)
						.Concat(RunManager.instance.levelCurrent.ModulesExtraction1)
						.Concat(RunManager.instance.levelCurrent.ModulesExtraction2)
						.Concat(RunManager.instance.levelCurrent.ModulesExtraction3)
					where x != null
					select string.Join(" - ", x.PrefabName.Split(" - ").Skip(2)).ToLower() into x
					orderby x
					select x).ToList().Distinct()
				where args.Length == 0 || x.StartsWith(string.Join(' ', args).ToLower())
				select x).ToList(), () => SemiFunc.IsMasterClientOrSingleplayer() && IsInGame()));
			Register(new ChatCommand("noaggro", "Toggle enemy vision for all players", delegate
			{
				enemyNoVision = !enemyNoVision;
				EnemyDirector.instance.debugNoVision = enemyNoVision;
				Debug.Log($"Command Used: /noaggro {enemyNoVision}");
				CommandSuccessEffect();
				DebugConsoleUI.instance?.SetResponseText(enemyNoVision ? "Enemy Vision: <color=red>Disabled</color>" : "Enemy Vision: <color=green>Enabled</color>", Color.white);
			}, null, () => SemiFunc.IsMasterClientOrSingleplayer() && IsInGame()));
			Register(new ChatCommand("noenemies", "Destroy all enemies", delegate
			{
				int num = 0;
				foreach (EnemyParent item5 in EnemyDirector.instance.enemiesSpawned.ToList())
				{
					if ((bool)item5)
					{
						num++;
						EnemyDirector.instance.enemiesSpawned.Remove(item5);
						if (SemiFunc.IsMultiplayer())
						{
							PhotonNetwork.Destroy(item5.gameObject);
						}
						else
						{
							UnityEngine.Object.Destroy(item5.gameObject);
						}
					}
				}
				Debug.Log("Command Used: /noenemies");
				CommandSuccessEffect();
				DebugConsoleUI.instance?.SetResponseText($"Enemies Destroyed: <color=green>{num}</color>", Color.white);
			}, null, () => SemiFunc.IsMasterClientOrSingleplayer() && !SemiFunc.RunIsLobbyMenu() && IsInGame()));
			Register(new ChatCommand("enemyspawnidle", "Toggle enemy spawn idle pause", delegate
			{
				EnemyDirector.instance.debugNoSpawnIdlePause = !EnemyDirector.instance.debugNoSpawnIdlePause;
				Debug.Log($"Command Used: /enemyspawnidle {!EnemyDirector.instance.debugNoSpawnIdlePause}");
				CommandSuccessEffect();
				DebugConsoleUI.instance?.SetResponseText(EnemyDirector.instance.debugNoSpawnIdlePause ? "Enemy Spawn Idle Timer: <color=red>Disabled</color>" : "Enemy Spawn Idle Timer: <color=green>Enabled</color>", Color.white);
			}, null, () => SemiFunc.IsMasterClientOrSingleplayer() && !SemiFunc.RunIsLobbyMenu() && IsInGame()));
			Register(new ChatCommand("revive", "Revive a player", delegate(bool isDebugConsole, string[] args)
			{
				string playerName = string.Join(' ', args).ToLower();
				if (playerName != "all")
				{
					PlayerAvatar playerAvatar = (string.IsNullOrWhiteSpace(playerName) ? PlayerAvatar.instance : GameDirector.instance.PlayerList.FirstOrDefault((PlayerAvatar x) => x.playerName.ToLower() == playerName));
					if (!playerAvatar)
					{
						CommandFailedEffect();
					}
					else if (!playerAvatar.deadSet)
					{
						DebugConsoleUI.instance?.SetResponseText("<color=red>" + playerAvatar.playerName + "</color> is not dead", Color.white);
						CommandFailedEffect();
					}
					else
					{
						playerAvatar.playerDeathHead.inExtractionPoint = true;
						playerAvatar.playerDeathHead.Revive();
						Debug.Log("Command Used: /revive " + playerAvatar.steamID);
						CommandSuccessEffect();
						DebugConsoleUI.instance?.SetResponseText("Revived: <color=green>" + playerAvatar.playerName + "</color>", Color.white);
					}
				}
				else
				{
					int num = 0;
					foreach (PlayerAvatar item6 in SemiFunc.PlayerGetList())
					{
						if ((bool)item6 && item6.deadSet)
						{
							item6.playerDeathHead.inExtractionPoint = true;
							item6.playerDeathHead.Revive();
							num++;
						}
					}
					Debug.Log("Command Used: /revive all");
					CommandSuccessEffect();
					DebugConsoleUI.instance?.SetResponseText($"Revived <color=green>{num}</color> players", Color.white);
				}
			}, (bool isDebugConsole, string partial, string[] args) => (args.Length > 1) ? new List<string>() : (from g in GameDirector.instance.PlayerList.Select((PlayerAvatar x) => x.playerName.ToLower()).Prepend("all").Distinct()
				where args.Length == 0 || g.StartsWith(args[0])
				select g).ToList(), () => SemiFunc.IsMasterClientOrSingleplayer() && !SemiFunc.RunIsLobbyMenu() && IsInGame()));
			Register(new ChatCommand("slow", "Toggle slow movement", delegate
			{
				PlayerController.instance.debugSlow = !PlayerController.instance.debugSlow;
				Debug.Log($"Command Used: /slow {PlayerController.instance.debugSlow}");
				CommandSuccessEffect();
				DebugConsoleUI.instance?.SetResponseText(PlayerController.instance.debugSlow ? "Slow Walk Speed: <color=green>Enabled</color>" : "Slow Walk Speed: <color=red>Disabled</color>", Color.white);
			}, null, () => !SemiFunc.RunIsLobbyMenu() && IsInGame()));
			Register(new ChatCommand("spawn", "Spawn an object at the nearest level point", delegate(bool isDebugConsole, string[] args)
			{
				if (args.Length == 0)
				{
					CommandFailedEffect();
				}
				else
				{
					string typedRest = string.Join(' ', args.Skip(1)).ToLower();
					switch (args[0])
					{
					case "enemy":
					{
						EnemySetup enemySetup = EnemyDirector.instance.enemiesDifficulty1.Concat(EnemyDirector.instance.enemiesDifficulty2).Concat(EnemyDirector.instance.enemiesDifficulty3).FirstOrDefault((EnemySetup x) => (bool)x && Regex.Replace(x.name, "^Enemy (- )?", "").ToLower() == typedRest);
						if ((bool)enemySetup)
						{
							LevelPoint levelPoint3 = SemiFunc.LevelPointsGetClosestToLocalPlayer();
							Debug.Log("Command Used: /spawn enemy " + typedRest);
							CommandSuccessEffect();
							EnemyDirector.instance.debugSpawnClose = true;
							foreach (PrefabRef spawnObject in enemySetup.spawnObjects)
							{
								GameObject gameObject = ((GameManager.instance.gameMode != 0) ? PhotonNetwork.InstantiateRoomObject(spawnObject.ResourcePath, levelPoint3.transform.position, Quaternion.identity, 0) : UnityEngine.Object.Instantiate(spawnObject.Prefab, levelPoint3.transform.position, Quaternion.identity));
								EnemyParent component = gameObject.GetComponent<EnemyParent>();
								if ((bool)component)
								{
									component.SetupDone = true;
									gameObject.GetComponentInChildren<Enemy>()?.EnemyTeleported(levelPoint3.transform.position);
									component.firstSpawnPointUsed = true;
								}
							}
							EnemyDirector.instance.debugSpawnClose = false;
							return;
						}
						break;
					}
					case "item":
					{
						Item item = StatsManager.instance.itemDictionary.Values.FirstOrDefault((Item x) => (bool)x && Regex.Replace(x.name, "^Item ", "").ToLower() == typedRest);
						if ((bool)item)
						{
							LevelPoint levelPoint2 = SemiFunc.LevelPointsGetClosestToLocalPlayer();
							Vector3 position2 = new Vector3(levelPoint2.transform.position.x, levelPoint2.transform.position.y + 1f, levelPoint2.transform.position.z);
							Debug.Log("Command Used: /spawn item " + typedRest);
							CommandSuccessEffect();
							if (GameManager.instance.gameMode == 0)
							{
								UnityEngine.Object.Instantiate(item.prefab.Prefab, position2, levelPoint2.transform.rotation);
							}
							else
							{
								PhotonNetwork.InstantiateRoomObject(item.prefab.ResourcePath, position2, levelPoint2.transform.rotation, 0);
							}
							return;
						}
						break;
					}
					case "valuable":
					{
						PrefabRef prefabRef = RunManager.instance.levels.SelectMany((Level l) => from x in l.ValuablePresets.SelectMany((LevelValuables p) => p.tiny.Concat(p.small).Concat(p.medium).Concat(p.big)
								.Concat(p.wide)
								.Concat(p.tall)
								.Concat(p.veryTall))
							where x.IsValid()
							select x).FirstOrDefault((PrefabRef x) => Regex.Replace(x.PrefabName, "^Valuable ", "").ToLower() == typedRest);
						if (prefabRef != null)
						{
							LevelPoint levelPoint = SemiFunc.LevelPointsGetClosestToLocalPlayer();
							Vector3 position = new Vector3(levelPoint.transform.position.x, levelPoint.transform.position.y + 1f, levelPoint.transform.position.z);
							Debug.Log("Command Used: /spawn valuable " + typedRest);
							CommandSuccessEffect();
							((GameManager.instance.gameMode != 0) ? PhotonNetwork.InstantiateRoomObject(prefabRef.ResourcePath, position, levelPoint.transform.rotation, 0) : UnityEngine.Object.Instantiate(prefabRef.Prefab, position, levelPoint.transform.rotation)).GetComponent<ValuableObject>().DollarValueSetLogic();
							return;
						}
						break;
					}
					}
					CommandFailedEffect();
				}
			}, delegate(bool isDebugConsole, string partial, string[] args)
			{
				List<string> source = new List<string> { "enemy", "item", "valuable" };
				if (args.Length <= 1)
				{
					return source.Where((string g) => args.Length == 0 || g.ToLower().StartsWith(args[0].ToLower())).ToList();
				}
				List<string> source2 = args[0] switch
				{
					"enemy" => (from x in EnemyDirector.instance.enemiesDifficulty1.Concat(EnemyDirector.instance.enemiesDifficulty2).Concat(EnemyDirector.instance.enemiesDifficulty3)
						where x
						select Regex.Replace(x.name, "^Enemy (- )?", "").ToLower()).Distinct().ToList(), 
					"item" => (from x in StatsManager.instance.itemDictionary.Values
						where x
						select Regex.Replace(x.name, "^Item ", "").ToLower()).Distinct().ToList(), 
					"valuable" => RunManager.instance.levels.SelectMany((Level l) => from x in l.ValuablePresets.SelectMany((LevelValuables p) => p.tiny.Concat(p.small).Concat(p.medium).Concat(p.big)
							.Concat(p.wide)
							.Concat(p.tall)
							.Concat(p.veryTall))
						where x.IsValid()
						select Regex.Replace(x.PrefabName, "^Valuable ", "").ToLower()).Distinct().ToList(), 
					_ => new List<string>(), 
				};
				string typedRest = ((args.Length > 1) ? string.Join(' ', args[1..]).ToLower() : "");
				if (!string.IsNullOrEmpty(typedRest))
				{
					source2 = source2.Where((string x) => x.StartsWith(typedRest)).ToList();
				}
				return source2.OrderBy((string x) => x).ToList();
			}, () => SemiFunc.IsMasterClientOrSingleplayer() && !SemiFunc.RunIsLobbyMenu() && IsInGame()));
			Register(new ChatCommand("startroom", "Spawn a specific start room", delegate(bool isDebugConsole, string[] args)
			{
				string typedRest = string.Join(' ', args).ToLower();
				PrefabRef prefabRef = RunManager.instance.levelCurrent.StartRooms.Where((PrefabRef x) => x != null).FirstOrDefault((PrefabRef x) => string.Join(" - ", x.PrefabName.Split(" - ").Skip(2)).ToLower() == typedRest);
				if (prefabRef != null)
				{
					moduleLevel = RunManager.instance.levelCurrent;
					moduleStartRoom = prefabRef;
					RunManager.instance.RestartScene();
					if (!SemiFunc.RunIsTutorial())
					{
						SemiFunc.OnSceneSwitch(RunManager.instance.gameOver, _leaveGame: false);
					}
					Debug.Log("Command Used: /startroom " + typedRest);
				}
				else
				{
					moduleStartRoom = null;
				}
				CommandSuccessEffect();
				DebugConsoleUI.instance?.SetResponseText((prefabRef != null) ? "Start Room Override: <color=green>Enabled</color>" : "Start Room Override: <color=red>Disabled</color>", Color.white);
			}, (bool isDebugConsole, string partial, string[] args) => (from x in (from x in RunManager.instance.levelCurrent.StartRooms
					where x != null
					select string.Join(" - ", x.PrefabName.Split(" - ").Skip(2)).ToLower()).Distinct()
				where args.Length == 0 || x.StartsWith(string.Join(' ', args).ToLower())
				orderby x
				select x).ToList(), () => SemiFunc.IsMasterClientOrSingleplayer() && IsInGame()));
			Register(new ChatCommand("upgrade", "Change your upgrade amount", delegate(bool isDebugConsole, string[] args)
			{
				if (args.Length == 0)
				{
					CommandFailedEffect();
				}
				else
				{
					string text = (from k in StatsManager.instance.dictionaryOfDictionaries.Keys
						where k.StartsWith("playerUpgrade")
						select k.Replace("playerUpgrade", "") into k
						orderby k
						select k).ToList().FirstOrDefault((string u) => u.ToLower() == args[0].ToLower());
					int result;
					if (text == null)
					{
						DebugConsoleUI.instance?.SetResponseText("No upgrade named '" + args[0] + "'.", Color.red);
						CommandFailedEffect();
					}
					else if (args.Length == 1 || !int.TryParse(args[1], out result))
					{
						CommandFailedEffect();
					}
					else
					{
						int num = StatsManager.instance.dictionaryOfDictionaries["playerUpgrade" + text][PlayerController.instance.playerSteamID];
						result = Math.Min(10000, num + result) - num;
						if (SemiFunc.IsMultiplayer())
						{
							PunManager.instance.photonView.RPC("TesterUpgradeCommandRPC", RpcTarget.All, PlayerController.instance.playerSteamID, text, result);
						}
						else
						{
							PunManager.instance.TesterUpgradeCommandRPC(PlayerController.instance.playerSteamID, text, result);
						}
						Debug.Log($"Command Used: /upgrade {text} {result}");
						CommandSuccessEffect();
						float num2 = StatsManager.instance.dictionaryOfDictionaries["playerUpgrade" + text][PlayerController.instance.playerSteamID];
						DebugConsoleUI.instance?.SetResponseText($"{text}: <color=green>{num2}</color>", Color.white);
					}
				}
			}, delegate(bool isDebugConsole, string partial, string[] args)
			{
				List<string> source = (from k in StatsManager.instance.dictionaryOfDictionaries.Keys
					where k.StartsWith("playerUpgrade")
					select k.Replace("playerUpgrade", "") into k
					orderby k
					select k).ToList();
				if (args.Length <= 1)
				{
					return source.Where((string g) => args.Length == 0 || g.ToLower().StartsWith(args[0].ToLower())).ToList();
				}
				return (args.Length == 2) ? new List<string> { "-1", "1" }.Where((string g) => g.StartsWith(args[1])).ToList() : new List<string>();
			}, () => IsInGame() && Debug.isDebugBuild));
		}
		if (SemiFunc.DebugDev())
		{
			Register(new ChatCommand("overlay", "Toggle debug overlay", delegate
			{
				debugOverlay = !debugOverlay;
				CommandSuccessEffect();
				DebugConsoleUI.instance?.SetResponseText(debugOverlay ? "Debug Overlay: <color=green>Enabled</color>" : "Debug Overlay: <color=red>Disabled</color>", Color.white);
			}, null, null, debugOnly: false));
		}
		Debug.Log($"Commands Loaded: {_commands.Count}");
	}

	private void Update()
	{
		if (!LevelGenerator.Instance || LevelGenerator.Instance.Generated || !(RunManager.instance?.levelCurrent == moduleLevel))
		{
			return;
		}
		if (moduleStartRoom != null && moduleStartRoom.IsValid())
		{
			LevelGenerator.Instance.DebugStartRoom = moduleStartRoom;
		}
		if (moduleObject != null && moduleObject.IsValid())
		{
			LevelGenerator.Instance.DebugModule = moduleObject;
			switch (moduleType)
			{
			case Module.Type.Normal:
				LevelGenerator.Instance.DebugNormal = true;
				break;
			case Module.Type.Passage:
				LevelGenerator.Instance.DebugPassage = true;
				LevelGenerator.Instance.DebugAmount = 6;
				break;
			case Module.Type.DeadEnd:
			case Module.Type.Extraction:
				LevelGenerator.Instance.DebugDeadEnd = true;
				LevelGenerator.Instance.DebugAmount = 1;
				break;
			}
		}
	}
}
