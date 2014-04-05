# Plugin ConferenceRegistrationFees

This is a plugin for [www.openpetra.org](http://www.openpetra.org).

## Functionality

You can import participants of a conference, that have been registered with another software.

This plugin will:
* read the main data of the participants from an *.ods file, transparent to the user
* the user only types in the registration numbers, and the amounts that should be paid (paste from Excel works fine)
* print a list of this current batch, with names of participants and bank account owner.
* create Emails from an HTML template and send them. The Emails announce the SEPA direct debit to the bank account owner.
* create the SEPA file for the SEPA direct Debit.
* create a gift batch export *.csv file. This will be imported into the old Petra for bookkeeping

## Dependencies

This plugin depends on:

* https://github.com/SolidCharity/OpenPetraPlugin_SEPA

## Installation

Please copy this directory to your OpenPetra working directory, to csharp\ICT\Petra\Plugins, 
and then run
    nant generateSolution

Please check the config directory for changes to your config files.

## License

This plugin is licensed under the GPL v3.