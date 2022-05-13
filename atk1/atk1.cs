using System;                          // Console
using System.IO;                       // MemoryStream
using System.Media;                    // SoundPlayer
using System.Runtime.InteropServices;  // Marshal

public class ATK1 {
  // 使用する AquesTalk 1 の DLL ファイルは棒読みちゃん同梱のモノでも SofTalk 内蔵のモノでも使えた
  const string dllPath = ".\\AquesTalk-softalk_v1_93_56-f1.dll";
  // const string dllPath = ".\\AquesTalk-bouyomichan_v0_1_11_0_beta21-f1.dll";
  
  [DllImport(dllPath)]
  private static extern IntPtr AquesTalk_Synthe(string koe, int iSpeed, ref int size);
  
  [DllImport(dllPath)]
  private static extern void AquesTalk_FreeWave(IntPtr wavPtr);
  
  public static void Main() {
    Console.WriteLine("Start");
    
    const int iSpeed = 100;
    const string koe = "こんにちわバージョンわん";
    Console.WriteLine("DLL   : {0}", dllPath);
    Console.WriteLine("Speed : {0}", iSpeed);
    Console.WriteLine("Text  : {0}", koe);
    
    // 音声ファイルとしてそのまま保存可能なバイト列の先頭ポイントを取得する
    int size = 0;
    IntPtr wavPtr = IntPtr.Zero;
    try {
      wavPtr = AquesTalk_Synthe(koe, iSpeed, ref size);  // throws
      
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
    AquesTalk_FreeWave(wavPtr);
    
    // 同期再生する
    using(var ms = new MemoryStream(wav))
    using(var sp = new SoundPlayer(ms)) {
      sp.PlaySync();
    }
    
    Console.WriteLine("Finished");
  }
  
}

// コンパイルと実行は以下のとおり (PowerShell にて)
// C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe /nologo /platform:x86 .\atk1.cs ; .\atk1.exe
// 
// 「/platform:x86」でプラットフォーム指定をしないと以下の例外が出る
// 
// ハンドルされていない例外: System.BadImageFormatException: 間違ったフォーマットのプログラムを読み込もうとしました。 (HRESULT からの例外:0x8007000B)
//    場所 HelloWorld.AquesTalk_Synthe(String koe, Int32 iSpeed, Int32& size)
//    場所 HelloWorld.Main()
