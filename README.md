# mkiso
�h���C�u�������̓f�B���N�g������ISO�t�@�C�����쐬���܂��B�ق�API�h�L�������g�̃T���v�����̂܂܂ł��B

## �v���b�g�t�H�[��
* Windows10�ȍ~�^.NET9 �ȍ~

## �r���h
* Visual studio 2022 or higher.
* Windows SDK commandline tools.

## ����
```
mkiso �T�u�R�}���h �R�}���h�p�����[�^�[
```
### �T�u�R�}���h:  
#### drv
CD/DVD/BD�h���C�u�ɑ}������Ă��郁�f�B�A����ISO�t�@�C�����_���v���܂��B

```
mkiso drv <drive letter> [out directory path]
```
�h���C�u���^�[(�R�����͕s�v:�G���[�`�F�b�N����)���w�肵�A�w�肵���f�B���N�g���Ƀ_���v�o�͂��܂��B  
(�t�@�C�����Ƀ{�����[�����x�����g�p���܂�)

�w��ł���h���C�u��CD-ROM��DVD���f�B�A�ȂǁB  
(�s�̂�DVD Video�Ȃǃv���e�N�g�����郁�f�B�A�͓��R�ł����G���[�ɂȂ�܂��B)

#### dir
�C�ӂ̃f�B���N�g������ISO�t�@�C�����쐬���܂��B (���ۂɃ��f�B�A�ɏĂ��邩�ǂ����͎����Ă݂܂���)

```
mkiso dir <dest path> <volume name> <src directory>
```
�f�B���N�g���p�X�����[�g�Ƃ���ISO�t�@�C�����o�͂��܂��B
