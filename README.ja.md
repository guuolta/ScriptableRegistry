# ScriptableRegistry

Unity のアセットを走査して、ファイル名から生成した Enum をキーに ScriptableObject の辞書へ自動登録するライブラリです。アセット追加・削除と列挙体の同期、辞書への登録をエディタ拡張でまとめて自動化し、ランタイムでは安全に `IReadOnlyDictionary` として参照できます。

## 特長
- フォルダ配下を再帰的に走査し、拡張子フィルタと無視フォルダで対象アセットを絞り込み
- ファイル名から Enum と、番号を安定化する JSON を生成
- 生成済み Enum を使い ScriptableObject 内の辞書へ自動登録（`ToDictionary()` でコピー取得も可能）
- `Window > ScriptableRegistry > CreateWindow` からレジストリ本体・カスタムエディタ・不足する Enum/値クラスをまとめて生成
- サンプル一式（Music/Block）を同梱し、動作例をすぐ確認可能
- 文字列生成は `USE_ZSTRING` シンボルで ZString に切り替え可能（未定義時は `System.Text.StringBuilder`）

## 動作環境
- Unity 6000.3 以降
- UPM のローカル/リモート（Git）参照に対応

## インストール
- **ローカルパス**: リポジトリをプロジェクト直下に置くか、`Packages/com.guuolta.ScriptableRegistry` のみコピー。Package Manager で **Add package from disk...** を選び `Packages/com.guuolta.ScriptableRegistry/package.json` を指定するか、`manifest.json` に追記します。
  ```json
  {
    "dependencies": {
      "com.guuolta.ScriptableRegistry": "file:Packages/com.guuolta.ScriptableRegistry"
    }
  }
  ```
- **Git (UPM)**: Package Manager で **Add package from git URL...** を選び、以下を入力するか `manifest.json` に追記します。
  - `https://github.com/guuolta/ScriptableRegistry.git?path=Packages/com.guuolta.ScriptableRegistry`
  ```json
  {
    "dependencies": {
      "com.guuolta.ScriptableRegistry": "https://github.com/guuolta/ScriptableRegistry.git?path=Packages/com.guuolta.ScriptableRegistry"
    }
  }
  ```

## クイックスタート
1. **（任意）スクリプトの自動生成**  
   `Window > ScriptableRegistry > CreateWindow` を開きます。  
   ![Window](https://github.com/guuolta/ScriptableRegistry/blob/image/Window.png)  
   主なパラメータ:
   - Script Name / Save Path / Namespace: レジストリ ScriptableObject のスクリプト名と保存先
   - Editor Script Name / Save Path / Namespace: カスタムエディタのスクリプト名と保存先
   - Menu Name / File Name: CreateAssetMenu 用のメニュー・ファイル名
   - Key Enum Name / Namespace: アセットファイル名に対応する Enum
   - Value Class Name / Namespace: 辞書に格納する値クラス
   - Target File Class Name: 取得対象のアセット型（例: `GameObject`, `AudioClip`）  
   Auto トグルで自動値を手動入力に切替可能。Key Enum/Value Class は存在しなければ生成、存在すれば上書きされます。

2. **レジストリアセットの作成**  
   生成された CreateAssetMenu（例: `ScriptableRegistry/...`）から ScriptableObject アセットを作成します。

3. **インスペクター設定**  
   ![SO](https://github.com/guuolta/ScriptableRegistry/blob/image/SO.png))  
   - Folder Path: 走査するフォルダ（末尾 `/` 不要）
   - Enum Path / Enum Namespace / Enum File Name: Enum/JSON の出力先と名前
   - File Extensions: 対象拡張子（カンマ区切り、例 `.prefab,.asset`）
   - Ignore Folder Names: 無視するフォルダ名
   - ボタン:
     - **Generate Enum / JSON**: Enum と JSON を生成（再コンパイルあり）
     - **Register Dictionary From Existing Enum**: 生成済み Enum を用いて辞書へ登録
     - **Reset**: 辞書をクリア

4. **ランタイムで参照**  
   ScriptableObject の `Dictionary` プロパティは `IReadOnlyDictionary` として公開され、`ToDictionary()` で通常の `Dictionary` を取得できます。

## スクリプトのカスタマイズ
![Window](https://github.com/guuolta/ScriptableRegistry/blob/image/Program.png)  
エディタ拡張は最小限の実装です。必要に応じて以下をオーバーライドして振る舞いを調整できます。
- `GetDefaultParams()`: Enum のデフォルト値など、初期設定を返す
- `CreateValue()`: 辞書に格納する値の生成方法をカスタマイズ

## ランタイム利用例
```csharp
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] private ScriptableRegistry.Sample.Music.MusicRegisterObject _registry;
    [SerializeField] private AudioSource _audioSource;

    public void Play(ScriptableRegistry.Sample.Music.MusicType type)
    {
        if (_registry.Dictionary.TryGetValue(type, out var clip))
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}
```

## サンプル
- `Assets/Sample` にスクリプトとアセットを同梱（Music は `AudioClip`、Block は `GameObject` からコンポーネント取得の例）。
- `Assets/Scenes/SampleScene.unity` を開くと動作例を確認できます。
- サンプル（Block/Music）フォルダ:
  ```
  Assets/Sample/
  ├─ Block/
  │  ├─ Block1.prefab
  │  ├─ Block2.prefab
  │  ├─ Block3.prefab
  │  ├─ IgnoreBlockFolder/…      # 無視フォルダ例
  │  └─ IgnoreBlockFolder2/…     # 無視フォルダ例
  ├─ Music/
  │  ├─ 1_軽いパンチ1.mp3
  │  ├─ K.O..mp3
  │  └─ 剣で斬る1.mp3
  ├─ Scripts/
  │  ├─ Block/
  │  │  ├─ BlockType.cs
  │  │  ├─ BlockType.json
  │  │  ├─ BlockRegisterObject.cs
  │  │  └─ BlockBehaviour.cs
  │  └─ Music/
  │     ├─ MusicType.cs
  │     ├─ MusicType.json
  │     └─ MusicRegisterObject.cs
  └─ ScriptableObject/
     ├─ BlockRegisterObject.asset
     └─ MusicRegisterObject.asset
  ```

## SerializableDictionary について
- Unity 既存のシリアライズでは `Dictionary` が扱えないため、`SerializableDictionary` を利用してインスペクターから内容を保持・編集可能にしています。
- `Dictionary` プロパティは `IReadOnlyDictionary` として公開されており、ランタイムで安全に参照できます。変更が必要な場合は `ToDictionary()` でコピーを取得してください。

## 開発・貢献
- バグ報告や要望は Issue へ、改善提案は Pull Request を歓迎します。
- パッケージは `Packages/com.guuolta.ScriptableRegistry` 配下にあり、エディタ関連は `Editor/`、実行時コードは `Runtime/` に配置されています。

## ライセンス
- MIT License（`LICENSE` を参照）

## 効果音について
本リポジトリには 効果音ラボ の効果音を同梱しています。
効果音ラボの利用規約に従って、サンプルの一部として使用しています。

配布元：効果音ラボ
利用規約：https://soundeffect-lab.info/agreement/

本リポジトリは効果音素材そのものの再配布を目的とするものではありません。
