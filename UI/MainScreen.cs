using Godot;
using System;
using SpriteUtility;

namespace DsocAssetUtilities.UI
{
    public sealed partial class MainScreen : Control
    {
        private const string PrefixInputDirectory = "Input Directory: ";
        private const string PrefixOutputDirectory = "Output Directory: ";

        private const string ErrNoInputDirectory = "No valid input directory selected.";
        private const string ErrNoOutputDirectory = "No valid output directory selected.";
        
        
        private OptionButton _dropdownModeSelector;
        private Button _buttonExecuteProgram;

        private FileDialog _fileDialogInputDirectorySelector;
        private FileDialog _fileDialogOutputDirectorySelector;

        private Button _buttonSelectInputDirectory;
        private Label _pvSelectedInputDirectory;
        private Button _buttonSelectOutputDirectory;
        private Label _pvSelectedOutputDirectory;
        

        private string _selectedInputDirectory = "";
        private string _selectedOutputDirectory = "";

        private SpriteProcessingMode _spriteProcessingMode = SpriteProcessingMode.OutputOptimizedData;
        

        public override void _Ready()
        {
            _dropdownModeSelector = GetNode<OptionButton>("%DropdownModeSelector");
            _buttonExecuteProgram = GetNode<Button>("%ButtonExecuteProgram");
            
            _fileDialogInputDirectorySelector = GetNode<FileDialog>("%FileDialogInputDirectorySelector");
            _fileDialogOutputDirectorySelector = GetNode<FileDialog>("%FileDialogOutputDirectorySelector");

            _buttonSelectInputDirectory = GetNode<Button>("%ButtonSelectInputDirectory");
            _pvSelectedInputDirectory = GetNode<Label>("%PvSelectedInputDirectory");
            _buttonSelectOutputDirectory = GetNode<Button>("%ButtonSelectOutputDirectory");
            _pvSelectedOutputDirectory = GetNode<Label>("%PvSelectedOutputDirectory");
            
            
            foreach (var mode in Enum.GetValues<SpriteProcessingMode>())
            {
                _dropdownModeSelector.AddItem(mode.ToString().Capitalize(), (int)mode);
            }
            
            _dropdownModeSelector.Select((int)_spriteProcessingMode);
            _dropdownModeSelector.ItemSelected += _onDropdownModeSelectorItemSelected;

            _fileDialogInputDirectorySelector.DirSelected += _onInputDirectorySelected;
            _fileDialogOutputDirectorySelector.DirSelected += _onOutputDirectorySelected;

            _buttonSelectInputDirectory.Pressed += () => _fileDialogInputDirectorySelector.PopupCentered();
            _buttonSelectOutputDirectory.Pressed += () => _fileDialogOutputDirectorySelector.PopupCentered();

            _buttonExecuteProgram.Pressed += ExecuteProgram;
            
            _updateDirectoryPreview();
            _checkIsProgramExecutionAllowed();
        }

        public void ExecuteProgram()
        {
            if (_checkIsProgramExecutionAllowed())
            {
                SpriteUtility.IO.CommonUtility.WriteAllSpriteData(_selectedInputDirectory, _selectedOutputDirectory, _spriteProcessingMode);
            }
        }

        private bool _checkIsProgramExecutionAllowed()
        {
            if (_checkDirectoryExists(_selectedInputDirectory) && _checkDirectoryExists(_selectedOutputDirectory))
            {
                if (_selectedInputDirectory != _selectedOutputDirectory)
                {
                    _buttonExecuteProgram.Disabled = false;
                    return true;
                }
            }

            _buttonExecuteProgram.Disabled = true;
            return false;
        }

        private void _onDropdownModeSelectorItemSelected(long index)
        {
            _spriteProcessingMode = (SpriteProcessingMode)_dropdownModeSelector.GetSelectedId();
        }

        private void _updateDirectoryPreview()
        {
            if (!_checkDirectoryExists(_selectedInputDirectory))
                _pvSelectedInputDirectory.Text = ErrNoInputDirectory;
            else
                _pvSelectedInputDirectory.Text = PrefixInputDirectory + _selectedInputDirectory;

            if (!_checkDirectoryExists(_selectedOutputDirectory))
                _pvSelectedOutputDirectory.Text = ErrNoOutputDirectory;
            else
                _pvSelectedOutputDirectory.Text = PrefixOutputDirectory + _selectedOutputDirectory;
        }

        private bool _checkDirectoryExists(string path)
        {
            return System.IO.Directory.Exists(path);
        }

        private void _onInputDirectorySelected(string dir)
        {
            _selectedInputDirectory = dir;
            _updateDirectoryPreview();
            _checkIsProgramExecutionAllowed();
        }

        private void _onOutputDirectorySelected(string dir)
        {
            _selectedOutputDirectory = dir;
            _updateDirectoryPreview();
            _checkIsProgramExecutionAllowed();
        }
    }
}
