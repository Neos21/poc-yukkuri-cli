using System;                          // Console
using System.IO;                       // MemoryStream
using System.Media;                    // SoundPlayer
using System.Runtime.InteropServices;  // Marshal

public class ATK2 {
  // SofTalk 同梱の AquesTalk2 の DLL で動いた
  const string dllPath = ".\\AquesTalk2-softalk_v1_93_56.dll";
  // SofTalk 同梱の AquesTalk2 の Phont を指定すると声質を変化させられる
  const string filePath = ".\\AquesTalk2-softalk_v1_93_56-ar_f4.phont";
  
  // phont ファイルを使用せず DLL 内蔵の音質 (DLL 内蔵の phont は ar_f4 の模様) だけ使う場合は「byte[] phontDat」部分を「int phontDat」と宣言し引数に 0 を与えれば良い
  [DllImport(dllPath)]
  private static extern IntPtr AquesTalk2_Synthe(string koe, int iSpeed, ref int size, byte[] phontDat);
  // private static extern IntPtr AquesTalk2_Synthe(string koe, int iSpeed, ref int size, int phontDat);
  
  [DllImport(dllPath)]
  private static extern void AquesTalk2_FreeWave(IntPtr wavPtr);
  
  public static void Main() {
    Console.WriteLine("Start");
    
    const int iSpeed = 100;
    const string koe = "こんにちわバージョンつー";
    Console.WriteLine("DLL   : {0}", dllPath);
    Console.WriteLine("Speed : {0}", iSpeed);
    Console.WriteLine("Text  : {0}", koe);
    
    // phont ファイルを読み込む
    FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
    byte[] buffer = new byte[fs.Length];
    int bytesRead = fs.Read(buffer, 0, buffer.Length);
    fs.Close();
    
    // 音声ファイルとしてそのまま保存可能なバイト列の先頭ポイントを取得する
    int size = 0;
    IntPtr wavPtr = IntPtr.Zero;
    try {
      wavPtr = AquesTalk2_Synthe(koe, iSpeed, ref size, buffer);  // throws
      // wavPtr = AquesTalk2_Synthe(koe, iSpeed, ref size, 0);  // throws
      
      // 失敗していれば終了する
      if(wavPtr == IntPtr.Zero) {
        Console.WriteLine("ERROR : 音声生成に失敗しました。不正な文字が使われた可能性があります。終了します");
        return;
      }
    }
    catch(Exception exception) {
      Console.WriteLine("ERROR : 例外が発生しました");
      Console.WriteLine(exception);
      Console.WriteLine("終了します");
      return;
    }
    
    // C# で扱えるようにマネージド側へコピーする
    byte[] wav = new byte[size];
    Marshal.Copy(wavPtr, wav, 0, size);
    
    // アンマネージドポインタは用がなくなった瞬間に解放する
    AquesTalk2_FreeWave(wavPtr);
    
    // 同期再生する
    using(var ms = new MemoryStream(wav))
    using(var sp = new SoundPlayer(ms)) {
      sp.PlaySync();
    }
    
    Console.WriteLine("Finished");
  }
  
}

// コンパイルと実行は以下のとおり (PowerShell にて)
// C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /nologo /platform:x86 .\atk2.cs ; .\atk2.exe
