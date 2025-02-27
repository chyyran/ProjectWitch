﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq; //iOSで動かないかも

namespace ProjectWitch.Battle
{
	// 顔グラの周囲全般を管理するオブジェクト
	public class FaceObj : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private GameObject mImFace = null, mImTargetFlame = null;
		[SerializeField]
		private GameObject mImActionArrow = null, mImSelectArrow = null;
		[SerializeField]
		private GameObject mHPTexts = null;
		[SerializeField]
		private GameObject mTeSolNum = null, mTeHP = null;
		[SerializeField]
		private GameObject mImSolNumBar = null, mImHPBar = null;
		[SerializeField]
		private GameObject mAttackButton = null, mDefenseButton = null, mCaptureButton = null;
		[SerializeField]
		private GameObject mNameText = null, mStateText = null;
		[SerializeField]
		private Transform mStatusContainer = null;
		[SerializeField]
		private GameObject mStatusUpDown = null, mStatusPoison = null, mStatusGuard = null, mStatusNoDamage = null;
		[SerializeField]
		private Sprite mSpriteStatusUp = null, mSpriteStatusDown = null,
			mSpriteStatusLead = null, mSpriteStatusAgi = null, mSpriteStatusPhyAtk = null, mSpriteStatusPhyDef = null, mSpriteStatusMagAtk = null, mSpriteStatusMagDef = null;
		[SerializeField]
		private GameObject mImLevel = null, mTeLevel = null;
		[SerializeField]
		private GameObject mGOSolNum = null;
		[SerializeField]
		private GameObject mImRace = null;
		[SerializeField]
		private List<Sprite> mSpriteRace = null;

		private int mHP, mSoldierNum;
		private Vector3 mBaseRectPos;
		private Vector2 mBaseRectSize;
		private List<float> mPreStatusDataUpDown;
		private List<bool> mPreStatusDataOther;
		private List<GameObject> Icons { get; set; }
		private List<GameObject> PrefabStatusOthers { get; set; }
		private List<Sprite> SpriteUpDownType { get; set; }

		public BattleObj BattleObj { get; private set; }
		public SkillButton AttackButton { get { return mAttackButton.GetComponent<SkillButton>(); } }
		public SkillButton DefenseButton { get { return mDefenseButton.GetComponent<SkillButton>(); } }
		public SkillButton CaptureButton { get { return mCaptureButton.GetComponent<SkillButton>(); } }
		public RectTransform Rect { get { return gameObject.GetComponent<RectTransform>(); } }
		public BattleUnit Unit { get; private set; }
		public bool IsExsistUnit { get; private set; }
		public int Pos { get; private set; }
		public bool IsPlayer { get; private set; }
		public bool IsMoving { get; private set; }
		public class BoolClass
		{
			public bool Flag { get; set; }
			public BoolClass() { Flag = false; }
			public BoolClass(bool _flag) { Flag = _flag; }
			public static bool operator true(BoolClass _flag) { return _flag.Flag; }
			public static bool operator false(BoolClass _flag) { return !_flag.Flag; }
			public static bool operator !(BoolClass _flag) { return !_flag.Flag; }
		}
		public BoolClass IsMoveArrowSelect { get; private set; }
		public BoolClass IsMoveArrowAction { get; private set; }
		// 音楽再生
		public PlayMusic Music { get { return BattleObj.Music; } }

		// ユニットを設定する
		public void SetUnit(BattleUnit unit, bool isPlayer)
		{
			Unit = unit;
			Unit.Face = this;
			IsMoveArrowSelect = new BoolClass();
			IsMoveArrowAction = new BoolClass();
			var image = mImFace.GetComponent<Image>();
			image.sprite = Resources.Load<Sprite>("Textures/Face/" + unit.UnitData.FaceIamgePath);
			if (!image.sprite)
				image.sprite = Resources.Load<Sprite>("Textures/Face/640_fv_non");
			mHPTexts.SetActive(true);

			SetHP();
			SetSolNum();

			mNameText.GetComponent<Text>().text = Unit.UnitData.Name;
			mStateText.GetComponent<Text>().text = "";

			AttackButton.Setup(this);
			DefenseButton.Setup(this);
			CaptureButton.Setup(this);
			HideButton();
			IsExsistUnit = true;
			Icons = new List<GameObject>();
			PrefabStatusOthers = new List<GameObject>();
			PrefabStatusOthers.Add(mStatusPoison);
			PrefabStatusOthers.Add(mStatusGuard);
			PrefabStatusOthers.Add(mStatusNoDamage);
			SpriteUpDownType = new List<Sprite>();
			SpriteUpDownType.Add(mSpriteStatusPhyAtk);
			SpriteUpDownType.Add(mSpriteStatusPhyDef);
			SpriteUpDownType.Add(mSpriteStatusMagAtk);
			SpriteUpDownType.Add(mSpriteStatusMagDef);
			SpriteUpDownType.Add(mSpriteStatusAgi);
			SpriteUpDownType.Add(mSpriteStatusLead);

			mPreStatusDataUpDown = Enumerable.Repeat<float>(0, 6).ToList();
			mPreStatusDataOther = Enumerable.Repeat<bool>(false, 3).ToList();

			SetStatusIcons();

			mImLevel.SetActive(true);
			mImFace.GetComponent<RectTransform>().localScale = new Vector3((isPlayer ? -1 : 1), 1, 1);
			mImLevel.SetActive(true);
			mTeLevel.GetComponent<Text>().text = Unit.UnitData.Level.ToString();
			mImRace.SetActive(true);
			mImRace.GetComponent<Image>().sprite = mSpriteRace[(int)Unit.UnitData.Job];
		}

		// 空ユニットを設定する
		public void SetNoneUnit()
		{
			Unit = null;

			AttackButton.Setup(this);
			DefenseButton.Setup(this);
			CaptureButton.Setup(this);
			HideButton();
			SetActionArrow(false);
			SetSelectArrow(false);

			IsExsistUnit = false;
			var image = mImFace.GetComponent<Image>();
			image.sprite = Resources.Load<Sprite>("Textures/Face/640_fv_non");
			mHPTexts.SetActive(false);
			mNameText.GetComponent<Text>().text = "";
			mStateText.GetComponent<Text>().text = "";
			mImLevel.SetActive(false);
			mImRace.SetActive(false);
		}

		// ステータスアイコンを設定する
		public void SetStatusIcons()
		{
			for (int i = 0; i < mPreStatusDataUpDown.Count; i++)
				mPreStatusDataUpDown[i] = Unit.GetStatusPercent(i);
			for (int i = 0; i < mPreStatusDataOther.Count; i++)
				mPreStatusDataOther[i] = Unit.GetStatusOtherFlag(i);

			foreach (var icon in Icons)
				Destroy(icon);
			Icons.Clear();
			int iconNum = 0;
			for (int i = 0; i < mPreStatusDataOther.Count; i++)
			{
				if (mPreStatusDataOther[i])
				{
					var prefab = PrefabStatusOthers[i];
					var icon = BattleData.Instantiate(prefab, prefab.name, mStatusContainer);
					icon.localPosition = new Vector3(-95 + (iconNum % 4) * 65, 210 + (iconNum / 4) * 85, 0);
					icon.localScale = PrefabStatusOthers[i].transform.localScale;
					Icons.Add(icon.gameObject);
					++iconNum;
				}
			}
			for (int i = 0; i < mPreStatusDataUpDown.Count; i++)
			{
				if (mPreStatusDataUpDown[i] != 0)
				{
					var icon = BattleData.Instantiate(mStatusUpDown, mStatusUpDown.name, mStatusContainer);
					icon.localPosition = new Vector3(-95 + (iconNum % 4) * 65, 210 + (iconNum / 4) * 85, 0);
					icon.localScale = mStatusUpDown.transform.localScale;
					var arrow = icon.Find("Arrow").GetComponent<Image>();
					var type = icon.Find("Type").GetComponent<Image>();
					var text = icon.Find("Text").GetComponent<Text>();
					arrow.sprite = (mPreStatusDataUpDown[i] > 0 ? mSpriteStatusUp : mSpriteStatusDown);
					type.sprite = SpriteUpDownType[i];
					text.text = (int)mPreStatusDataUpDown[i] + "%";
					Icons.Add(icon.gameObject);
					++iconNum;
				}
			}
		}

		// 場所を設定する
		public void SetPos(int pos, bool isPlayer)
		{
			IsMoving = false;
			IsPlayer = isPlayer;
			Pos = pos;
			gameObject.GetComponent<RectTransform>().localPosition = new Vector3((mBaseRectPos.x + mBaseRectSize.x * Pos) * (IsPlayer ? -1 : 1), mBaseRectPos.y, mBaseRectPos.z);
		}

		// 場所を移動するコルーチン
		private IEnumerator CoMovePos(int pos)
		{
			IsMoving = true;
			var rect = gameObject.GetComponent<RectTransform>();
			float speedPerSec = 20f * BattleObj.BattleSpeedMagni;
			var targetPosX = (mBaseRectPos.x + mBaseRectSize.x * pos) * (IsPlayer ? -1 : 1);
			var isLeft = (IsPlayer ? Pos < pos : pos < Pos);
			while (isLeft ? rect.localPosition.x > targetPosX : rect.localPosition.x < targetPosX)
			{
				rect.localPosition += new Vector3(speedPerSec * (isLeft ? -1 : 1), 0, 0);
				yield return null;
			}
			SetPos(pos, IsPlayer);
		}

		// 場所を移動する
		public void MovePos(int pos)
		{
			if (pos == Pos)
				return;
			StartCoroutine("CoMovePos", pos);
			if (pos == 0)
				Unit.Position = Position.Front;
			else if (pos == 1)
				Unit.Position = Position.Middle;
			else
				Unit.Position = Position.Rear;
		}

		public void SetRemove(Color color, string text)
		{
			var image = mImFace.GetComponent<Image>();
			image.color = color;
			mHPTexts.SetActive(false);
			mImLevel.SetActive(false);
			mStateText.SetActive(true);
			mStateText.GetComponent<Text>().text = text;
			IsExsistUnit = false;
		}

		// 死亡設定
		public void SetDead()
		{
			SetRemove(new Color(255 / 255.0f, 47 / 255.0f, 47 / 255.0f), "死亡");
		}

		// 捕獲設定
		public void SetCapture()
		{
			SetRemove(new Color(47 / 255.0f, 47 / 255.0f, 255 / 255.0f), "捕獲");
		}

		// 撤退設定
		public void SetRetreat()
		{
			SetRemove(new Color(47 / 255.0f, 47 / 255.0f, 47 / 255.0f), "敗走");
		}

		// ボタンの名称を設定する
		public void SetButton(string attack, string defense, bool isCapture)
		{
			int pos = 0;
			if (isCapture && mCaptureButton != null)
				CaptureButton.SetButton("捕獲する", pos++);
			if (defense != "")
				DefenseButton.SetButton(defense, pos++);
			if (attack != "")
				AttackButton.SetButton(attack, pos++);
		}

		// ボタンを全て非表示にする
		public void HideButton()
		{
			AttackButton.HideButton();
			DefenseButton.HideButton();
			CaptureButton.HideButton();
		}

		void Awake()
		{
			mBaseRectPos = gameObject.GetComponent<RectTransform>().localPosition;
			mBaseRectSize = gameObject.GetComponent<RectTransform>().sizeDelta;
			mImActionArrow.SetActive(false);
			mImSelectArrow.SetActive(false);
			mImTargetFlame.SetActive(false);
		}

		// Use this for initialization
		void Start()
		{
			BattleObj = GameObject.Find("/BattleObject").GetComponent<BattleObj>();
			mImSelectArrow.SetActive(false);
		}

		private void SetHP()
		{
			mHP = Unit.DisplayHP;
			mTeHP.GetComponent<Text>().text = Unit.DisplayHP.ToString();
			var image = mImHPBar.GetComponent<Image>();
			image.fillAmount = 0;
			if (Unit.UnitData.MaxHP != 0)
				image.fillAmount = (float)Unit.DisplayHP / Unit.UnitData.MaxHP;
		}

		private void SetSolNum()
		{
			mGOSolNum.SetActive(Unit.UnitData.MaxSoldierNum != 0);
			if (mGOSolNum.activeSelf)
			{
				mSoldierNum = Unit.DisplaySoldierNum;
				mTeSolNum.GetComponent<Text>().text = Unit.DisplaySoldierNum.ToString();
				var image = mImSolNumBar.GetComponent<Image>();
				image.fillAmount = (float)Unit.DisplaySoldierNum / Unit.UnitData.MaxSoldierNum;
			}
		}

		// Update is called once per frame
		void Update()
		{
			if (!Unit)
				return;
			if (mHP != Unit.DisplayHP)
				SetHP();
			if (mSoldierNum != Unit.DisplaySoldierNum)
				SetSolNum();
			// ステータスアイコン変更
			bool statusChangeFlag = false;
			for (int i = 0; i < mPreStatusDataUpDown.Count; i++)
			{
				if (mPreStatusDataUpDown[i] != Unit.GetStatusPercent(i))
				{
					statusChangeFlag = true;
					break;
				}
			}
			if (!statusChangeFlag)
			{
				for (int i = 0; i < mPreStatusDataOther.Count; i++)
				{
					if (mPreStatusDataOther[i] != Unit.GetStatusOtherFlag(i))
					{
						statusChangeFlag = true;
						break;
					}
				}
			}
			if (statusChangeFlag)
				SetStatusIcons();
		}

		// マウスオーバーしたときの動作
		public void OnPointerEnterAction()
		{
			if (!BattleObj.IsPlayerSelectTime || !IsExsistUnit || Unit.UnitData.HP == 0)
				return;
			var turnUnit = BattleObj.TurnUnit;
			string atkSkillName = "", defSkillName = "";
			bool isCapture = (!Unit.IsPlayer && Unit.UnitData.Catchable && !Unit.IsExistSoldier);
			if (turnUnit.IsPlayer == Unit.IsPlayer)
			{
				// 味方
				if (turnUnit == Unit && turnUnit.LDefSkill.Target == SkillDataFormat.SkillTarget.Own)
					defSkillName = turnUnit.LDefSkill.Name;
				else if (turnUnit.LDefSkill.Target == SkillDataFormat.SkillTarget.Player ||
					turnUnit.LDefSkill.Target == SkillDataFormat.SkillTarget.EnemyAndPlayer ||
					turnUnit.LDefSkill.Target == SkillDataFormat.SkillTarget.PlayerLeader)
				{
					defSkillName = turnUnit.LDefSkill.Name;
					if (turnUnit.LDefSkill.Range == SkillDataFormat.SkillRange.All)
						foreach (var face in (Unit.IsPlayer ? BattleObj.PlayerFaces : BattleObj.EnemyFaces))
							if (face.Unit != Unit && face.IsExsistUnit)
							{
								face.SetButton("", defSkillName, false);
								face.SetSelectArrow(true);
							}
				}
			}
			else
			{
				// 敵
				if (turnUnit.LAtkSkill.Target == SkillDataFormat.SkillTarget.Enemy ||
					turnUnit.LAtkSkill.Target == SkillDataFormat.SkillTarget.EnemyAndPlayer ||
					turnUnit.LAtkSkill.Target == SkillDataFormat.SkillTarget.EnemyLeader)
				{
					atkSkillName = turnUnit.LAtkSkill.Name;
					if (turnUnit.LAtkSkill.Range == SkillDataFormat.SkillRange.All)
						foreach (var face in (Unit.IsPlayer ? BattleObj.PlayerFaces : BattleObj.EnemyFaces))
							if (face.Unit != Unit && face.IsExsistUnit)
							{
								face.SetButton(atkSkillName, "", false);
								face.SetSelectArrow(true);
							}
				}
				if (turnUnit.LDefSkill.Target == SkillDataFormat.SkillTarget.Enemy ||
					turnUnit.LDefSkill.Target == SkillDataFormat.SkillTarget.EnemyAndPlayer ||
					turnUnit.LDefSkill.Target == SkillDataFormat.SkillTarget.EnemyLeader)
				{
					defSkillName = turnUnit.LDefSkill.Name;
					if (turnUnit.LDefSkill.Range == SkillDataFormat.SkillRange.All)
						foreach (var face in (Unit.IsPlayer ? BattleObj.PlayerFaces : BattleObj.EnemyFaces))
							if (face.Unit != Unit && face.IsExsistUnit)
							{
								face.SetButton("", defSkillName, false);
								face.SetSelectArrow(true);
							}
				}
			}
			SetButton(atkSkillName, defSkillName, isCapture);
			if (!Unit.IsPlayer)
				Unit.StartCoroutine("SlideIn");
			SetSelectArrow(true);
			Music.PlayOverUnitFace();
		}

		// マウスオーバーした時
		public void OnPointerEnter(PointerEventData e)
		{
			OnPointerEnterAction();
		}

		// マウスが離されたとき
		public void OnPointerExit(PointerEventData e)
		{
			if (!BattleObj.IsPlayerSelectTime || !IsExsistUnit)
				return;
			SetAllFaceHideButton();
			if (!Unit.IsPlayer)
				Unit.StartCoroutine("SlideOut");
		}

		// 選択指定を表示
		public void SetSelectArrow(bool flag)
		{
			mImTargetFlame.SetActive(flag);
			if (IsMoveArrowSelect == null)
				IsMoveArrowSelect = new BoolClass();
			IsMoveArrowSelect.Flag = flag;
			if (flag)
				StartCoroutine(CoMoveArrow(mImSelectArrow, IsMoveArrowSelect));
		}

		// 行動設定
		public void SetActionArrow(bool action)
		{
			if (IsMoveArrowAction == null)
				IsMoveArrowAction = new BoolClass();
			IsMoveArrowAction.Flag = action;
			if (action)
				StartCoroutine(CoMoveArrow(mImActionArrow, IsMoveArrowAction));
		}

		private IEnumerator CoMoveArrow(GameObject _arrow, BoolClass _flag)
		{
			_arrow.SetActive(true);
			bool isMoveUp = true;
			float time = 0;
			var arrow = _arrow.GetComponent<RectTransform>();
			var pos = arrow.localPosition;
			while (_flag)
			{
				time += Time.deltaTime;
				var width = BattleObj.ArrowShakeWidth * Time.deltaTime * (isMoveUp ? 1 : -1);
				arrow.localPosition += new Vector3(0, width, 0);
				if (time >= BattleObj.ArrowTime)
				{
					time = 0;
					isMoveUp = !isMoveUp;
				}
				yield return null;
			}
			arrow.localPosition = pos;
			_arrow.SetActive(false);
		}

		public void SetAllFaceHideButton()
		{
			foreach (var face in BattleObj.PlayerFaces)
				if (face.IsExsistUnit)
				{
					face.HideButton();
					face.SetSelectArrow(false);
				}
			foreach (var face in BattleObj.EnemyFaces)
				if (face.IsExsistUnit)
				{
					face.HideButton();
					face.SetSelectArrow(false);
				}
		}
	}
}