﻿//==========================================================
//	author	:shotta
//	summary	:Gameクラスとのアダプタ
//==========================================================

using UnityEngine;

using System.Collections.Generic;
using System;
using System.Reflection;

using ProjectWitch.Extention;
using System.Linq;

namespace ProjectWitch.Talk.WorkSpace
{
    //自己破壊を待つ基底アップデータ
    public class SelfDeleteUpdater : UpdaterFormat
    {
        public override void Setup()
        {
        }

        public override void Update(float deltaTime)
        {
            SetActive(false);
        }

        public override void Finish()
        {
        }
    }

    //タイマーウェイトのアップデータ
    public class TimerWaitUpdater : SelfDeleteUpdater
    {
        float mTime = 0.0f;

        protected TimerWaitUpdater() { }
        public TimerWaitUpdater(float time)
        {
            mTime = time;
        }

        public override void Setup()
        {
        }

        public override void Update(float deltaTime)
        {
            mTime -= deltaTime;
            if (mTime <= 0.0f)
                SetActive(false);
        }

        public override void Finish()
        {
        }
    }
    
    //シナリオ操作時のワークスペース
    public class GameWorkSpace : MonoBehaviour
	{
		//battle_unit_inタグ
		//	p		:p?引数、0~2
		//	e		:e?引数、0~2
		//	error	:エラーメッセージ
		void BattleUnitIn(int[] p, int[] e, out string error)
		{
			error = null;

            var game = Game.GetInstance();
            for(int i=0; i<3; i++)
            {
                game.BattleIn.PlayerUnits[i] = p[i];
            }

            for(int i=0; i<3; i++)
            {
                game.BattleIn.EnemyUnits[i] = e[i];
            }

            game.BattleIn.IsEvent = true;
		}

        //battle_areaタグ
        //	id		:戦闘領地を指定
        //	error	:エラーメッセージ
        void BattleArea(int id, out string error)
        {
            var game = Game.GetInstance();

            game.BattleIn.AreaID = id;

            error = null;

        }

        //battle_nonpreタグ
        //	error	:エラーメッセージ
        void BattleNonPre(out string error)
		{
			error = null;
            var game = Game.GetInstance();

            //戦闘準備画面を用いない
            game.UsePreBattle = false;

        }

		//battle_autoタグ
		//	error	:エラーメッセージ
		void BattleAuto(out string error)
		{
			error = null;
            var game = Game.GetInstance();

            //次の戦闘をオート戦闘に
            game.BattleIn.IsAuto = true;

		}

		//call_endingタグ
		//	index	:エンディングのインデックス
		//	error	:エラーメッセージ
		void CallEnding(int index, out string error)
		{
            var game = Game.GetInstance();

            StartCoroutine(game.CallEnding(index));

			error = null;

		}

		//if_aliveタグ
		//	unitIds	:指定されたユニットのIDの配列
		//	error	:エラーメッセージ
		bool IfAlive(int[] unitIDs, out string error)
		{
			error = null;
            var game = Game.GetInstance();
            
            foreach(var unitID in unitIDs)
            {
                try
                {
                    if (!game.GameData.Unit[unitID].IsAlive)
                        return false;
                }
                catch(ArgumentException e)
                {
                    error = "unitIDが存在しない範囲の値です";
                    error += e.Message;
                }
            }

			return true;
		}

		//if_deathタグ
		//	unitIds	:指定されたユニットのIDの配列
		//	error	:エラーメッセージ
		bool IfDeath(int[] unitIDs, out string error)
		{
			error = null;
            var game = Game.GetInstance();

            foreach(var unitID in unitIDs)
            {
                try
                {
                    if (game.GameData.Unit[unitID].IsAlive)
                        return false;
                }
                catch(ArgumentException e)
                {
                    error = "unitIDが存在しない範囲の値です";
                    error += e.Message;
                }
            }
			return true;
		}

		//unit_heal_allタグ
		//	territory:指定された領地のID
		//	error	 :エラーメッセージ
		void UnitHealAll(int territory, out string error)
		{
			error = null;
            var game = Game.GetInstance();

            //指定領地のユニットIDをすべて受け取る
            var groupIDs = game.GameData.Territory[territory].GroupList;
            var groups = game.GameData.Group.GetFromIndex(groupIDs);
            var unitIDs = new List<int>();
            foreach (var group in groups)
                unitIDs.AddRange(group.UnitList);
            unitIDs = unitIDs.Distinct().ToList();
            
            //ユニットを回復させる
            foreach(var unitID in unitIDs)
            {
                try
                {
                    game.GameData.Unit[unitID].HP = game.GameData.Unit[unitID].MaxHP;
                    game.GameData.Unit[unitID].SoldierNum = game.GameData.Unit[unitID].MaxSoldierNum;
                }
                catch(ArgumentException e)
                {
                    error = "unitIDが存在しない範囲の値です:";
                    error += e.Message;
                }
            }


		}

		//unit_healタグ
		//	unitIds	:指定されたユニットのIDの配列
		//	error	:エラーメッセージ
		void UnitHeal(int[] unitIDs, out string error)
		{
			error = null;
            var game = Game.GetInstance();

            foreach(var unitID in unitIDs)
            {
                try
                {
                    game.GameData.Unit[unitID].HP = game.GameData.Unit[unitID].MaxHP;
                    game.GameData.Unit[unitID].SoldierNum = game.GameData.Unit[unitID].MaxSoldierNum;
                }
                catch(ArgumentException e)
                {
                    error = "unitIDが存在しない範囲です";
                    error += e.Message;
                }
            }

		}

		//unit_killタグ
		//	unitIds	:指定されたユニットのIDの配列
		//	error	:エラーメッセージ
		void UnitKill(int[] unitIDs, out string error)
		{
			error = null;
            var game = Game.GetInstance();

            foreach (var unitID in unitIDs)
            {
                game.GameData.Unit[unitID].IsAlive = false;
            }

            UnitUnemploy(unitIDs, out error);
        }
        
        //ユニットを解雇する
        void UnitUnemploy(int[] unitIDs, out string error)
        {
            error = null;
            var game = Game.GetInstance();

            foreach (var unitID in unitIDs)
            {
                //自分の領地からユニットを除外
                game.GameData.Territory[0].RemoveUnit(unitID);
            }
        }

		//unit_employタグ
		//	unitIds	:指定されたユニットのIDの配列
		//	error	:エラーメッセージ
		void UnitEmploy(int[] unitIDs, out string error)
		{
			error = null;
            var game = Game.GetInstance();
            var territory = game.GameData.Territory[0];

            foreach (var unitID in unitIDs)
            {
                //全領地からユニットを除外
                foreach (var ter in game.GameData.Territory)
                    ter.RemoveUnit(unitID);

                //ユニットを自領地に含める
                territory.AddUnit(unitID);
            }

        }

        [SerializeField]
        private GameObject mTalkController = null;

        //インスペクターから登録してくだせ
        [SerializeField]
        private ScenarioWorkSpace mSWS = null;
        
        //スタート時に仮想マシンへ割り込みする処理
        private class ScriptBeginAnimator : PauseUpdater
        {
            //継続時間
            float mTime = 0.0f;
            //現在の時間
            private float mCurrentTime = 0.0f;

            public ScriptBeginAnimator(float time)
            {
                mTime = time;
            }

            //初期化処理
            public override void Setup()
            {
            }
            //更新処理
            public override void Update(float deltaTime)
            {
                mCurrentTime += deltaTime;
                if (mCurrentTime > mTime)
                {
                    SetActive(false);//アップデータを抜ける処理
                }
            }
            //終了処理
            public override void Finish()
            {

            }
        }

        [SerializeField]
        private float mBeginTime = 0.0f;

        //スクリプト開始
        public void ScriptBegin()
        {
            //割り込みを登録
            mSWS.SetUpdater(new ScriptBeginAnimator(mBeginTime));
            Debug.Log("開始");
        }
        
        //スクリプト終了
        public void ScriptEnd()
        {
           StartCoroutine(mTalkController.GetComponent<TalkController>().EndScript());
        }

        //change_owner_areaタグ
        //	id		:指定された領地のID
        //	error	:エラーメッセージ
        void ChangeAreaOwner(int area, int owner, out string error)
        {
            var game = Game.GetInstance();

            game.ChangeAreaOwner(area, owner);

            error = null;

        }

        public void SetCommandDelegaters(VirtualMachine vm)
		{
			//システム変数
			//取得
			vm.AddCommandDelegater (
				"GetSystem",
				new CommandDelegater (true, 1, delegate(object[] arguments) {
					string error;
					int index = Converter.ObjectToInt(arguments[0], out error);
					if (error != null) return error;

					VirtualMemory memory = Game.GetInstance().GameData.Memory;
					int count = memory.Count;
					if (0 <= index && index<count){
						arguments[1] = memory[index];
						return null;
					}
					return "システム変数のインデックスは0 ~ "+ (count - 1) +"です(" + index + ")";
				}));
			//入力
			vm.AddCommandDelegater (
				"SetSystem",
				new CommandDelegater (false, 2, delegate(object[] arguments) {
					string error;
					int index = Converter.ObjectToInt(arguments[0], out error);
					if (error != null) return error;

					VirtualMemory memory = Game.GetInstance().GameData.Memory;
					int count = memory.Count;
					object value = arguments[1];
					if (0 <= index && index<count){
						memory[index] = int.Parse(value.ToString());
						return null;
					}
					return "システム変数のインデックスは0 ~ "+ (count - 1) +"です(" + index + ")";
				}));

			//戦闘データのセット
			vm.AddCommandDelegater (
				"SetBattleUnit",
				new CommandDelegater (false, 6, delegate(object[] arguments) {
					string error;
					int[] p = new int[3];
					p[0] = Converter.ObjectToInt(arguments[0], out error);
					if (error != null) return error;
					p[1] = Converter.ObjectToInt(arguments[1], out error);
					if (error != null) return error;
					p[2] = Converter.ObjectToInt(arguments[2], out error);
					if (error != null) return error;

					int[] e = new int[3];
					e[0] = Converter.ObjectToInt(arguments[3], out error);
					if (error != null) return error;
					e[1] = Converter.ObjectToInt(arguments[4], out error);
					if (error != null) return error;
					e[2] = Converter.ObjectToInt(arguments[5], out error);
					if (error != null) return error;

					BattleUnitIn(p, e, out error);
					return error;
				}));

            //戦闘データのセット
            vm.AddCommandDelegater(
                "SetBattleArea",
                new CommandDelegater(false, 1, delegate (object[] arguments) {
                    string error;
                    int id = Converter.ObjectToInt(arguments[0], out error);
                    if (error != null) return error;
                    BattleArea(id, out error);
                    return error;
                }));

            vm.AddCommandDelegater(
                "SetBattleTime",
                new CommandDelegater(false, 1, delegate (object[] arguments)
                {
                    string error;
                    int time = Converter.ObjectToInt(arguments[0], out error);
                    if (error != null) return error;

                    var game = Game.GetInstance();
                    game.BattleIn.TurnNum = time;

                    return error;
                }));

            //編成画面を表示しない
            vm.AddCommandDelegater (
				"SetBattleNonPre",
				new CommandDelegater (false, 0, delegate(object[] arguments) {
					string error;
					BattleNonPre(out error);
					return error;
				}));
			
			//オートバトルを有効にする
			vm.AddCommandDelegater (
				"SetBattleAuto",
				new CommandDelegater (false, 0, delegate(object[] arguments) {
					string error;
					BattleAuto(out error);
					return error;
				}));

            //バトルを行わない
            vm.AddCommandDelegater(
                "SetBattleDisable",
                new CommandDelegater(false, 0, delegate (object[] arguments)
                {
                    Game.GetInstance().BattleIn.IsEnable = false;
                    return null;
                }));

            //バトルBGMを一時的に変更する
            vm.AddCommandDelegater(
                "SetBattleBGM",
                new CommandDelegater(false, 1, delegate (object[] arguments)
                {
                    string error;
                    var name = Converter.ObjectToString(arguments[0], out error);
                    if (error != null) return error;

                    Game.GetInstance().BattleIn.BGM = name;
                    return error;
                }));

			//エンディング呼び出し
			vm.AddCommandDelegater (
				"CallEnding",
				new CommandDelegater (false, 1, delegate(object[] arguments) {
					string error;
					int index = Converter.ObjectToInt(arguments[0], out error);
					if (error != null) return error;
					CallEnding (index, out error);
					return error;
				}));

            vm.AddCommandDelegater(
                "GameClear",
                new CommandDelegater(false, 0, delegate (object[] arguments)
                  {
                      string error = null;
                      var game = Game.GetInstance();
                      game.SystemData.Memory[0] = 1;

                      return error;
                  }));

			//指定キャラの生存フラグ
			vm.AddCommandDelegater (
				"IfAlive",
				new CommandDelegater (true, 1, delegate(object[] arguments) {
					string error;
					List<object> list = Converter.ObjectToList(arguments[0], out error);
					if (error != null) return error;

					int[] unitIds = new int[list.Count];
					for(int i=0;i<list.Count;i++)
					{
						unitIds[i] = Converter.ObjectToInt(list[i], out error);
						if (error != null) return error;
					}
					if (IfAlive(unitIds, out error))
						arguments[1] = 1;
					else
						arguments[1] = 0;
					return error;
				}));

			//指定キャラの死亡フラグ
			vm.AddCommandDelegater (
				"IfDeath",
				new CommandDelegater (true, 1, delegate(object[] arguments) {
					string error;
					List<object> list = Converter.ObjectToList(arguments[0], out error);
					if (error != null) return error;

					int[] unitIds = new int[list.Count];
					for(int i=0;i<list.Count;i++)
					{
						unitIds[i] = Converter.ObjectToInt(list[i], out error);
						if (error != null) return error;
					}
					if (IfDeath(unitIds, out error))
						arguments[1] = 1;
					else
						arguments[1] = 0;
					return error;
				}));
			
			//指定領地の全ユニットを回復
			vm.AddCommandDelegater (
				"HealAllUnit",
				new CommandDelegater (false, 1, delegate(object[] arguments) {
					string error;
					int territoryId = Converter.ObjectToInt(arguments[0], out error);
					if (error != null) return error;
					UnitHealAll(territoryId, out error);
					return error;
				}));

			//指定ユニットの体力を回復
			vm.AddCommandDelegater (
				"HealUnit",
				new CommandDelegater (false, 1, delegate(object[] arguments) {
					string error;
					List<object> list = Converter.ObjectToList(arguments[0], out error);
					if (error != null) return error;

					int[] unitIds = new int[list.Count];
					for(int i=0;i<list.Count;i++)
					{
						unitIds[i] = Converter.ObjectToInt(list[i], out error);
						if (error != null) return error;
					}
					UnitHeal(unitIds, out error);
					return error;
				}));

			//指定ユニットを解雇
			vm.AddCommandDelegater (
				"FireUnit",
				new CommandDelegater (false, 1, delegate(object[] arguments) {
					string error;
					List<object> list = Converter.ObjectToList(arguments[0], out error);
					if (error != null) return error;

					int[] unitIds = new int[list.Count];
					for(int i=0;i<list.Count;i++)
					{
						unitIds[i] = Converter.ObjectToInt(list[i], out error);
						if (error != null) return error;
					}
					UnitUnemploy(unitIds, out error);
					return error;
				}));

			//指定ユニットを解雇
			vm.AddCommandDelegater (
				"EmployUnit",
				new CommandDelegater (false, 1, delegate(object[] arguments) {
					string error;
					List<object> list = Converter.ObjectToList(arguments[0], out error);
					if (error != null) return error;

					int[] unitIds = new int[list.Count];
					for(int i=0;i<list.Count;i++)
					{
						unitIds[i] = Converter.ObjectToInt(list[i], out error);
						if (error != null) return error;
					}
					UnitEmploy(unitIds, out error);
					return error;
				}));

            //ユニットデータの変更
            vm.AddCommandDelegater(
                "ChangeUnitData",
                new CommandDelegater(false, 3, delegate (object[] arguments)
                {
                    string error = null;
                    string type = "";

                    int id = Converter.ObjectToInt(arguments[0], out error);
                    if (error != null) return error;
                    string member = Converter.ObjectToString(arguments[1], out error);
                    if (error != null) return error;
                    object value = Converter.AutoConvert(arguments[2], out type, out error);
                    if (error != null) return error;

                    //リフレクションを用いて、ユニットデータをセット
                    var game = Game.GetInstance();
                    var t = Type.GetType("ProjectWitch.UnitDataFormat");

                    try
                    {
                        t.GetProperty(member).SetValue(game.GameData.Unit[id], value, null);
                    }
                    catch(TargetException)
                    {
                        error = "メンバの型と値の型が一致しません。値の型：" + type;
                    }

                    return error;
                }));

            //戦闘データのセット
            vm.AddCommandDelegater(
                "ChangeAreaOwner",
                new CommandDelegater(false, 2, delegate (object[] arguments) {
                    string error = null;
                    int area = Converter.ObjectToInt(arguments[0], out error);
                    if (error != null) return error;
                    int owner = Converter.ObjectToInt(arguments[1], out error);
                    if (error != null) return error;
                    ChangeAreaOwner(area, owner, out error);
                    return error;
                }));

            //指定秒数だけウェイトを挟む
            vm.AddCommandDelegater(
                "WaitTimer",
                new CommandDelegater(true, 1, delegate (object[] arguments)
                {
                    string error = null;
                    var millisec = Converter.ObjectToInt(arguments[0], out error);
                    if (error != null) return error;

                    arguments[1] = new TimerWaitUpdater((float)millisec / 1000.0f);

                    return error;
                }));

            //カーソルの表示非表示
            vm.AddCommandDelegater(
                "ShowCursor",
                new CommandDelegater(false, 0, delegate (object[] arguments)
                {
                    Game.GetInstance().TalkCommand.ShowCursor();
                    return null;
                }));
            vm.AddCommandDelegater(
                "HideCursor",
                new CommandDelegater(false, 0, delegate (object[] arguments)
                {
                    Game.GetInstance().TalkCommand.HideCursor();
                    return null;
                }));

            //強調カーソルの表示非表示
            vm.AddCommandDelegater(
                "ShowAccentCursor",
                new CommandDelegater(false, 2, delegate (object[] arguments)
                {
                    string error = null;
                    var posx = Converter.ObjectToInt(arguments[0], out error);
                    if (error != null) return error;
                    var posy = Converter.ObjectToInt(arguments[1], out error);
                    if (error != null) return error;

                    Game.GetInstance().TalkCommand.ShowAccentCursor(new Vector2(posx, posy));

                    return error;
                }));
            vm.AddCommandDelegater(
                "HideAccentCursor",
                new CommandDelegater(false, 0, delegate (object[] arguments)
                {
                    Game.GetInstance().TalkCommand.HideAccentCursor();
                    return null;
                }));

            vm.AddCommandDelegater(
                "EnableBattleTutorial",
                new CommandDelegater(false, 0, delegate (object[] arguments)
                {
                    Game.GetInstance().TalkCommand.EnableBattleTutorial();
                    return null;
                }));

            vm.AddCommandDelegater(
                "EnableMenuTutorial",
                new CommandDelegater(false, 0, delegate (object[] arguments)
                 {
                     Game.GetInstance().TalkCommand.EnableMenuTutorial();
                     return null;
                 }));

        }
    }
}