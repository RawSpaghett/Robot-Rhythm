using System;
using UnityEngine;
using UnityEngine.UI;
using RobotRhythm.UI;

public class PracticeIntro : MonoBehaviour
{
    [SerializeField] private GameObject practicePad;
    [SerializeField] private Transform canvasRoot;
    [SerializeField] private Font font;
    [SerializeField] private Sprite pothole;
    [SerializeField] private Sprite duckObstacle;
    [SerializeField] private Sprite raisedBlock;
    [SerializeField] private Sprite holdMarker;
    private int holdExamples;
    private GameObject panel, pad, recap;
    private Image obstaclePicture;
    private Text title, instructions, progress, cueCaption;
    private DirectionArrow cue;
    private Button start;
    private Action beginLevel;
    private int step, ducks;

    public void Show(Action begin)
    {
        beginLevel = begin; step = 0; ducks = 0; holdExamples = 0;
        panel = new GameObject("PracticeIntroPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasRoot, false);
        Stretch((RectTransform)panel.transform);
        panel.GetComponent<Image>().color = new Color32(244,237,218,255);
        Label("INTRODUCTION", new Vector2(.5f,.86f), new Vector2(800,65),38);
        title = Label("",new Vector2(.28f,.72f),new Vector2(550,70),34);
        obstaclePicture = Picture(panel.transform,pothole,new Vector2(.28f,.57f));
        instructions = Label("",new Vector2(.28f,.36f),new Vector2(650,120),23);
        progress = Label("",new Vector2(.28f,.22f),new Vector2(540,40),22);
        pad = Instantiate(practicePad,panel.transform); pad.name="PracticePad";
        var rect = (RectTransform)pad.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(.76f,.52f);
        rect.pivot = new Vector2(.5f,.5f); rect.anchoredPosition=Vector2.zero;
        rect.sizeDelta=new Vector2(300,220);
        pad.GetComponent<GestureButtonInput>().GesturePerformed += OnGesture;
        var icon = new GameObject("Action cue",typeof(RectTransform),typeof(DirectionArrow));
        icon.transform.SetParent(panel.transform,false); cue=icon.GetComponent<DirectionArrow>();
        cue.rectTransform.anchorMin=cue.rectTransform.anchorMax=new Vector2(.76f,.72f);
        cue.rectTransform.sizeDelta=new Vector2(40,36);
        cue.color=new Color32(23,49,60,255); cue.raycastTarget=false;
        cueCaption=Label("",new Vector2(.76f,.28f),new Vector2(440,70),21);
        recap=new GameObject("Obstacle recap",typeof(RectTransform));
        recap.transform.SetParent(panel.transform,false); Stretch((RectTransform)recap.transform);
        RecapCard(pothole,"JUMP","Tap and release",.22f);
        RecapCard(duckObstacle,"DUCK","Drag down, keep pressed, then release",.5f);
        RecapCard(raisedBlock,"LONG JUMP","Drag up, keep pressed, then release",.78f);
        MakeButton("PracticeBack","BACK",new Vector2(.14f,.12f),()=>Destroy(panel));
        start=MakeButton("PracticeStart","START LEVEL",new Vector2(.77f,.12f),Finish);
        MakeButton("PracticeSkip","SKIP PRACTICE",new Vector2(.48f,.12f),Finish);
        UpdateStep();
    }
    private void OnGesture(GestureData gesture)
    {
        if(step==0 && (gesture.IsTap || gesture.Direction==SwipeDirection.Up)) step++;
        else if(step==1 && gesture.Direction==SwipeDirection.Down) step++;
        else if(step==2 && gesture.Direction==SwipeDirection.Down) { ducks++; if(ducks==3)step++; }
        else if(step==3 && gesture.Direction==SwipeDirection.Up && gesture.IsHold) step++;
        else if (step == 4 && gesture.IsHold && gesture.Direction == (holdExamples == 0 ? SwipeDirection.Up : SwipeDirection.Down))
        {
            holdExamples++;
            if (holdExamples == 2) step++;
        }
        UpdateStep();
    }
    private void UpdateStep()
    {
        string[] titles={"1. JUMP","2. DUCK","3. THREE DUCKS","4. LONG JUMP","5. HOLD","READY?"};
        string[] details={"Tap and release to jump.","Drag down and keep pressed.\nRelease to duck.",
            "Drag down, keep pressed, then release.\nRepeat three times with a new press.",
            "Drag up and keep pressed.\nRelease to jump farther.",
            "HOLD means keep the pad pressed.\nDrag in the arrow direction and stay there.\nRelease as the obstacle reaches your robot.",
            "Release as the obstacle reaches your robot.\nCloser timing earns more points."};
        bool finished=step==5;
        recap.SetActive(finished); pad.SetActive(!finished);
        cue.gameObject.SetActive(step>0 && !finished);
        cue.rectTransform.localRotation=Quaternion.Euler(0,0,(step==3 || (step==4 && holdExamples==0))?90:-90); cue.SetDouble(true);
        cueCaption.gameObject.SetActive(!finished);
        cueCaption.text=step==0?"TAP":(step==3 || (step==4 && holdExamples==0))?"UP / KEEP PRESSED / RELEASE":"DOWN / KEEP PRESSED / RELEASE";
        obstaclePicture.gameObject.SetActive(!finished);
        obstaclePicture.sprite=step==4?holdMarker:step==0?pothole:step==3?raisedBlock:duckObstacle;
        title.rectTransform.anchorMin=title.rectTransform.anchorMax=new Vector2(finished?.5f:.28f,.72f);
        instructions.rectTransform.anchorMin=instructions.rectTransform.anchorMax=new Vector2(finished?.5f:.28f,finished?.25f:.36f);
        title.text=titles[step]; instructions.text=details[step];
        progress.text=step==2?ducks+" / 3":step==4?holdExamples+" / 2":finished?"":"Try it on the pad";
        start.gameObject.SetActive(finished);
    }
    private void Finish(){Destroy(panel);beginLevel?.Invoke();}
    private void Stretch(RectTransform rect)
    {
        rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
    }
    private Image Picture(Transform parent,Sprite sprite,Vector2 anchor)
    {
        var image=new GameObject("Obstacle example",typeof(RectTransform),typeof(Image)).GetComponent<Image>();
        image.transform.SetParent(parent,false);image.rectTransform.anchorMin=image.rectTransform.anchorMax=anchor;
        image.rectTransform.sizeDelta=new Vector2(250,100);image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;
        return image;
    }
    private void RecapCard(Sprite sprite,string action,string gesture,float x)
    {
        Picture(recap.transform,sprite,new Vector2(x,.55f));
        Label(action,new Vector2(x,.43f),new Vector2(300,45),26).transform.SetParent(recap.transform,false);
        Label(gesture,new Vector2(x,.37f),new Vector2(310,55),19).transform.SetParent(recap.transform,false);
    }
    private Text Label(string value,Vector2 anchor,Vector2 size,int fontSize)
    {
        var text=new GameObject("PracticeText",typeof(RectTransform),typeof(Text)).GetComponent<Text>();
        text.transform.SetParent(panel.transform,false);text.rectTransform.anchorMin=text.rectTransform.anchorMax=anchor;
        text.rectTransform.sizeDelta=size;text.font=font;text.fontSize=fontSize;text.text=value;
        text.color=new Color32(23,49,60,255);text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
        return text;
    }
    private Button MakeButton(string name,string label,Vector2 anchor,UnityEngine.Events.UnityAction action)
    {
        var text=Label(label,anchor,new Vector2(270,72),24);
        var obj=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));
        obj.transform.SetParent(panel.transform,false);var rect=(RectTransform)obj.transform;
        rect.anchorMin=rect.anchorMax=anchor;rect.sizeDelta=new Vector2(280,76);
        var image=obj.GetComponent<Image>();image.color=new Color32(242,190,88,255);
        var button=obj.GetComponent<Button>();button.targetGraphic=image;button.onClick.AddListener(action);
        text.transform.SetParent(obj.transform,false);text.rectTransform.anchorMin=text.rectTransform.anchorMax=new Vector2(.5f,.5f);
        text.rectTransform.anchoredPosition=Vector2.zero;return button;
    }
}
