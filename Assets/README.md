

# Creaps FPS - Documentation

![Creaps FPS](/Assets/Documentation/Wallpaper.jpg "Wallpaper") 

>## `Proje İçeriği`
>
>Creaps FPS, Fusion(Photon Engine) Kullanarak hazırlanmış 3D Multiplayer FPS oyunudur.

## `Nasıl Oyanır?`
>
> MainMenu -> Play -> Start Game (Host) 
> 
> Oyun başladığında sahnede yok etmeniz gereken nesneler oluşacaktır.Bu nesneleri Gerekli mermi tipine göre yok etmelisiniz.
> Orta üst tarafta gereken mermi tipi belirtilir ve mermi tipinizi sağ ve solda bulunan size ve color yazılı butonlar ile değiştirebilirsiniz.
> 
> Her doğru yok edilen obje için +10 puan her yanlışınız için -10 puan kazanırsınız.
> 
> Oyun 10 Turdan oluşur. En yüksek puanı alan kişi oyunu kazanmış olur.
>

## `Oynanış Videosu`
> [Senior Game Youtube](https://www.youtube.com/watch?v=lP8CzQPL9PU)
>


## `Klasör Yapısı`
>
>  ### `_CreapsFPS` Klasörü;
> 
> > `Images` : Oyunda UI için kullanılan assetlere erişebilirsiniz.
> > 
> > `Materials` : Oyunun genelinde kullanılan materyallere ve skybox'a ulaşabilirsiniz.
> >
> > `Prefabs` : Oyunda kullanılan butün prefablara ve her prefabın özel olarak kullandığı materyaller ve görsellere ulaşabilirsiniz.
> >
> > `Scenes` : Oyunda aktif olarak kullanılan sahnelere ulaşabilirsiniz.
> >
> > `Scripts` : Oyunu tarafımca eklenmiş scriptlere ulaşabilirsiniz.
>
> `Company Assets` klasöründe proje ayarları için kullanılan splash icon ve arka plan resimlerini içerir.
>
> `Photon` klasöründe Multiplay için gereki olan Fusion(Photon Engine) [Fusion 1.1.1 Download](https://dashboard.photonengine.com/en-us/download/fusion/Photon-Fusion-1.1.1-F-512.unitypackage) dosyalarını içerir.
>
> `Plugin` klasöründe -> DoTween MEC Textmesh PRO ve Markdown Viewer gibi Yardımcı paketleri içerir.

 ![Project](/Assets/Documentation/Proje.png "Project")

## `Proje İçeriği Düzenlenmesi`

>  ##  `Gameplay sahnesi ;`
>
> > #### GameplayDataManager da karakterlerin spawn pointleri ayarlanabilir.
> >
> > ![SpawnPlayer](/Assets/Documentation/SpawnPlayer.png "SpawnPlayer")
> >
> > #### Target Shoot Game Control'den hedeflerin spawn pointleri ayarlanabilir.Total Stage Numb ile tur sayısı belirtilebilir.Stage time limits değişkenler ile bir turun ne kadar süreceği değiştirilebilir.
> >
> > ![SpawnTarget](/Assets/Documentation/SpawnTargets.png "SpawnTarget")
> > 
> > #### Joysticklerin işlevlerini buradan değiştirebilirsiniz.Butonların basma limitini bekleme sürelerini değiştirebilirsiniz. Joytickler UI Player Controllers Manager'a bağlıdır.
> >
> > ![Joystick](/Assets/Documentation/Joystick.png "Joystick")
> 
> 
> 
> 
> 